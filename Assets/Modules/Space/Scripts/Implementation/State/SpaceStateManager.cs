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
    /// Space profile on disk. Queues live here, not in the flight ECS world.
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
