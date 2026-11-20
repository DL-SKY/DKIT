using System;
using System.Collections.Generic;
using System.IO;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.State
{
    /// <summary>
    /// Space profile on disk. The hull, fitted modules, and queues live here, not in the flight ECS world.
    /// </summary>
    public sealed class SpaceStateManager
    {
        public const string SAVE_FOLDER = "Space";
        public const string PROFILE_FILE_NAME = "profile.json";

        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.Indented
        };

        private SpaceDefinitionsManager _definitions;

        public string ProfileDirectory { get; private set; }
        public string ProfileFilePath { get; private set; }
        public bool IsReady { get; private set; }
        public SpaceProfile Profile { get; private set; }

        public void Init(SpaceDefinitionsManager definitions)
        {
            if (string.IsNullOrEmpty(Application.persistentDataPath))
                throw new InvalidOperationException("Application.persistentDataPath is empty.");

            _definitions = definitions;
            ProfileDirectory = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            Directory.CreateDirectory(ProfileDirectory);
            ProfileFilePath = Path.Combine(ProfileDirectory, PROFILE_FILE_NAME);
            Profile = LoadOrCreate();
            EnsureQueues();
            if (!EnsureShip())
            {
                IsReady = false;
                return;
            }

            Save();
            IsReady = true;
        }

        public bool TrySetMaxCount(string queueId, int maxCount)
        {
            if (maxCount < 1 || !TryGetQueue(queueId, out var queue))
                return false;

            queue.MaxCount = maxCount;
            Save();
            return true;
        }

        public bool TryEnqueue(string queueId, string targetId, IReadOnlyList<SpaceTimeModifier> modifiers, long nowUnixMs)
        {
            if (_definitions == null || !TryGetQueue(queueId, out var queue))
                return false;

            if (queue.Rows.Count >= queue.MaxCount)
                return false;

            if (!SpaceQueueIds.TryGetKind(queueId, out var queueKind))
                return false;

            if (!_definitions.TryGetTimedTarget(targetId, out var target) || target.Kind != queueKind)
                return false;

            var row = new SpaceQueueRow
            {
                Kind = target.Kind,
                TargetId = target.Id,
                BaseTimeMs = target.BaseTimeMs,
                Modifiers = CopyModifiers(modifiers)
            };

            var hasActiveRow = false;
            for (var i = 0; i < queue.Rows.Count; i++)
            {
                if (queue.Rows[i].IsStarted)
                {
                    hasActiveRow = true;
                    break;
                }
            }

            if (!hasActiveRow)
                row.Start(nowUnixMs);

            queue.Rows.Add(row);
            Save();
            return true;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(ProfileFilePath) || Profile == null)
                return;

            var json = JsonConvert.SerializeObject(Profile, SETTINGS);
            File.WriteAllText(ProfileFilePath, json);
        }

        private SpaceProfile LoadOrCreate()
        {
            if (!File.Exists(ProfileFilePath))
                return new SpaceProfile();

            var json = File.ReadAllText(ProfileFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return new SpaceProfile();

            var profile = JsonConvert.DeserializeObject<SpaceProfile>(json, SETTINGS);
            return profile ?? new SpaceProfile();
        }

        private void EnsureQueues()
        {
            if (Profile.Queues == null)
                Profile.Queues = new List<SpaceQueueState>();

            for (var i = 0; i < SpaceQueueIds.ALL.Length; i++)
            {
                var queueId = SpaceQueueIds.ALL[i];
                if (TryGetQueue(queueId, out var queue))
                {
                    if (queue.MaxCount < 1)
                        queue.MaxCount = SpaceQueueIds.FREE_MAX_COUNT;
                    if (queue.Rows == null)
                        queue.Rows = new List<SpaceQueueRow>();
                    continue;
                }

                Profile.Queues.Add(new SpaceQueueState
                {
                    Id = queueId,
                    MaxCount = SpaceQueueIds.FREE_MAX_COUNT
                });
            }
        }

        private bool EnsureShip()
        {
            if (Profile.Ships == null)
                Profile.Ships = new List<SpacePlayerShipState>();

            Profile.Ships = KeepKnownShips(Profile.Ships);
            if (FindShip(Profile.ActiveShipId) == null)
            {
                if (Profile.Ships.Count == 0)
                {
                    var starter = CreateShip(SpaceProfile.STARTER_HULL_ID);
                    if (starter == null)
                        return false;

                    Profile.Ships.Add(starter);
                }

                Profile.ActiveShipId = Profile.Ships[0].Id;
            }

            for (var i = 0; i < Profile.Ships.Count; i++)
            {
                var ship = Profile.Ships[i];
                if (!_definitions.TryGetHull(ship.HullId, out var hull))
                    continue;

                ship.Slots = RebuildSlots(hull, ship.Slots);
                EnsureDefaultEngine(ship);
            }

            return FindShip(Profile.ActiveShipId) != null;
        }

        private SpacePlayerShipState CreateShip(string hullId)
        {
            if (!_definitions.TryGetHull(hullId, out _))
                return null;

            return new SpacePlayerShipState
            {
                Id = NewShipId(),
                Name = string.Empty,
                HullId = hullId
            };
        }

        private static string NewShipId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private List<SpacePlayerShipState> KeepKnownShips(List<SpacePlayerShipState> ships)
        {
            var kept = new List<SpacePlayerShipState>();
            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship == null || string.IsNullOrEmpty(ship.HullId) || !_definitions.TryGetHull(ship.HullId, out _))
                    continue;

                if (string.IsNullOrEmpty(ship.Id) || FindIn(kept, ship.Id) != null)
                    ship.Id = NewShipId();

                if (ship.Name == null)
                    ship.Name = string.Empty;

                kept.Add(ship);
            }

            return kept;
        }

        private List<SpacePlayerShipSlot> RebuildSlots(SpaceHullDef hull, List<SpacePlayerShipSlot> saved)
        {
            var slots = new List<SpacePlayerShipSlot>();
            if (hull?.Slots == null)
                return slots;

            var used = new bool[saved != null ? saved.Count : 0];
            for (var i = 0; i < hull.Slots.Count; i++)
            {
                var defined = hull.Slots[i];
                if (defined == null)
                    continue;

                var slot = new SpacePlayerShipSlot
                {
                    SlotType = defined.SlotType,
                    SizeType = defined.SizeType
                };
                slot.ModuleId = TakeInstalledModule(saved, used, defined);
                slots.Add(slot);
            }

            return slots;
        }

        private string TakeInstalledModule(List<SpacePlayerShipSlot> saved, bool[] used, SpaceHullSlotDef defined)
        {
            if (saved == null)
                return null;

            for (var i = 0; i < saved.Count; i++)
            {
                if (used[i])
                    continue;

                var previous = saved[i];
                if (previous == null || previous.SlotType != defined.SlotType || previous.SizeType != defined.SizeType)
                    continue;

                used[i] = true;
                if (string.IsNullOrEmpty(previous.ModuleId))
                    return null;

                if (!_definitions.TryGetModule(previous.ModuleId, out var module))
                    return null;

                if (module.SlotType != defined.SlotType || !module.AllowsSlotSize(defined.SizeType))
                    return null;

                return module.Id;
            }

            return null;
        }

        private void EnsureDefaultEngine(SpacePlayerShipState ship)
        {
            if (ship?.Slots == null || !_definitions.TryGetModule(SpaceProfile.STARTER_ENGINE_ID, out var engine))
                return;

            if (engine.SlotType != SpaceSlotType.Engine || !engine.IsUsable())
                return;

            for (var i = 0; i < ship.Slots.Count; i++)
            {
                var slot = ship.Slots[i];
                if (slot == null || slot.SlotType != SpaceSlotType.Engine || !string.IsNullOrEmpty(slot.ModuleId))
                    continue;

                if (!engine.AllowsSlotSize(slot.SizeType))
                    continue;

                slot.ModuleId = engine.Id;
                return;
            }
        }

        private SpacePlayerShipState FindShip(string shipId)
        {
            return FindIn(Profile?.Ships, shipId);
        }

        private static SpacePlayerShipState FindIn(List<SpacePlayerShipState> ships, string shipId)
        {
            if (ships == null || string.IsNullOrEmpty(shipId))
                return null;

            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship != null && ship.Id == shipId)
                    return ship;
            }

            return null;
        }

        private bool TryGetQueue(string queueId, out SpaceQueueState queue)
        {
            queue = null;
            if (Profile?.Queues == null || string.IsNullOrEmpty(queueId))
                return false;

            for (var i = 0; i < Profile.Queues.Count; i++)
            {
                var candidate = Profile.Queues[i];
                if (candidate != null && candidate.Id == queueId)
                {
                    queue = candidate;
                    return true;
                }
            }

            return false;
        }

        private static List<SpaceTimeModifier> CopyModifiers(IReadOnlyList<SpaceTimeModifier> modifiers)
        {
            var copy = new List<SpaceTimeModifier>();
            if (modifiers == null)
                return copy;

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier == null)
                    continue;

                copy.Add(new SpaceTimeModifier
                {
                    Kind = modifier.Kind,
                    Value = modifier.Value
                });
            }

            return copy;
        }
    }
}
