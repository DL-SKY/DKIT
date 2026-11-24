using System.Collections.Generic;
using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using Modules.ECS.Scripts.Space.Systems;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.State;
using Modules.Utils.Scripts.Components;
using UnityEngine;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Keeps the sector world alive after the startup scene unloads.
    /// </summary>
    public sealed class SpaceSessionHost : MonoBehaviour
    {
        private SpaceSessionController _session;
        private Transform _playerShip;
        private readonly List<GameObject> _spawnedObjects = new List<GameObject>();
        private EcsEntity[] _entityBuffer;

        public bool TrySetMoveTarget(string markerKey)
        {
            var world = _session != null ? _session.World : null;
            if (world == null || !world.IsAlive())
            {
                UnityEngine.Debug.LogError("[Space] Move target was not set. The sector world is not running.");
                return false;
            }

            if (string.IsNullOrEmpty(markerKey) || !TryFindMarker(world, markerKey, out var target))
            {
                UnityEngine.Debug.LogError($"[Space] Move target was not set. Marker '{markerKey}' was not found.");
                return false;
            }

            if (!target.Has<SpacePosition>())
            {
                UnityEngine.Debug.LogError($"[Space] Move target was not set. Marker '{markerKey}' has no position.");
                return false;
            }

            var targetPosition = target.Get<SpacePosition>().Value;
            var count = world.GetAllEntities(ref _entityBuffer);
            for (var i = 0; i < count; i++)
            {
                var entity = _entityBuffer[i];
                if (!entity.Has<SpacePlayerHull>() || !entity.Has<SpacePosition>())
                    continue;

                if (entity.Equals(target))
                {
                    UnityEngine.Debug.LogError("[Space] Move target was not set. The player hull cannot fly to itself.");
                    return false;
                }

                ref var bodyPosition = ref entity.Get<SpacePosition>();
                entity.Get<SpaceMoveTarget>() = new SpaceMoveTarget { Target = target };
                if (!SpaceBearing.TryDegrees(bodyPosition.Value, targetPosition, out var degrees))
                    degrees = entity.Has<SpaceYaw>() ? entity.Get<SpaceYaw>().Degrees : 0f;

                entity.Get<SpaceDesiredYaw>() = new SpaceDesiredYaw { Degrees = degrees };
                UnityEngine.Debug.Log($"[Space] Player move target set to '{markerKey}'.");
                return true;
            }

            UnityEngine.Debug.LogError("[Space] Move target was not set. The player hull entity is missing.");
            return false;
        }

        public bool TryStopPlayer()
        {
            var world = _session != null ? _session.World : null;
            if (world == null || !world.IsAlive())
            {
                UnityEngine.Debug.LogError("[Space] Player was not stopped. The sector world is not running.");
                return false;
            }

            var count = world.GetAllEntities(ref _entityBuffer);
            for (var i = 0; i < count; i++)
            {
                var entity = _entityBuffer[i];
                if (!entity.Has<SpacePlayerHull>())
                    continue;

                if (entity.Has<SpaceYaw>())
                    entity.Get<SpaceDesiredYaw>() = new SpaceDesiredYaw { Degrees = entity.Get<SpaceYaw>().Degrees };

                var speed = entity.Has<SpaceMotion>() ? entity.Get<SpaceMotion>().MetersPerSecond : 0f;
                if (speed > 0.0001f)
                    entity.Get<SpaceMoveTarget>() = new SpaceMoveTarget();
                else if (entity.Has<SpaceMoveTarget>())
                    entity.Del<SpaceMoveTarget>();

                UnityEngine.Debug.Log("[Space] Player route cleared. Speed will fall off by inertia.");
                return true;
            }

            UnityEngine.Debug.LogError("[Space] Player was not stopped. The player hull entity is missing.");
            return false;
        }

        private bool TryFindMarker(EcsWorld world, string markerKey, out EcsEntity target)
        {
            target = default;
            var count = world.GetAllEntities(ref _entityBuffer);
            for (var i = 0; i < count; i++)
            {
                var entity = _entityBuffer[i];
                if (!entity.Has<SpaceMarkerKey>() || entity.Get<SpaceMarkerKey>().Value != markerKey)
                    continue;

                target = entity;
                return true;
            }

            return false;
        }

        public void StartSession(
            Updater updater,
            EcsSystemFactory factory,
            SpaceDefinitionsManager definitions,
            SpaceStateManager state,
            string sectorId)
        {
            if (_session != null)
                return;

            _session = new SpaceSessionController(updater, factory);
            _session.Start();
            if (_session.World == null)
                return;

            if (!SpaceSectorLoad.TryGet(definitions, sectorId, out var sector))
            {
                UnityEngine.Debug.LogError($"[Space] Sector '{sectorId}' was not placed. The definition is missing.");
                return;
            }

            _spawnedObjects.AddRange(SpaceSectorBodies.Spawn(_session.World, definitions, sector));
            SpawnPlayerShip(definitions, state, sector);
        }

        public void CopyMarkers(List<SpaceMarkerSnapshot> target)
        {
            target.Clear();
            var world = _session != null ? _session.World : null;
            if (world == null || !world.IsAlive())
                return;

            var count = world.GetAllEntities(ref _entityBuffer);
            for (var i = 0; i < count; i++)
            {
                var entity = _entityBuffer[i];
                if (!entity.Has<SpacePosition>() || !entity.Has<SpaceLabel>() || !entity.Has<SpaceMarkerKey>())
                    continue;

                var player = entity.Has<SpacePlayerHull>();
                var ship = entity.Has<SpaceHull>();
                SpaceObjectType objectType;
                SpaceRelation relation;
                if (player || ship)
                {
                    objectType = SpaceObjectType.Ship;
                    relation = player ? SpaceRelation.Player : SpaceRelation.Neutral;
                }
                else if (entity.Has<SpaceBodyType>())
                {
                    objectType = entity.Get<SpaceBodyType>().Value;
                    relation = SpaceRelation.Neutral;
                }
                else
                {
                    continue;
                }

                if (objectType == SpaceObjectType.Spawn)
                    continue;

                var label = entity.Get<SpaceLabel>().Value;
                var metersPerSecond = 0f;
                var maxMetersPerSecond = 0f;
                if (entity.Has<SpaceMotion>())
                {
                    var motion = entity.Get<SpaceMotion>();
                    metersPerSecond = motion.MetersPerSecond;
                    maxMetersPerSecond = motion.MaxMetersPerSecond;
                }

                target.Add(new SpaceMarkerSnapshot
                {
                    TargetId = entity.Get<SpaceMarkerKey>().Value,
                    DefId = label,
                    Ship = ship,
                    Player = player,
                    ObjectType = objectType,
                    Relation = relation,
                    WorldPosition = entity.Get<SpacePosition>().Value,
                    MetersPerSecond = metersPerSecond,
                    MaxMetersPerSecond = maxMetersPerSecond
                });
            }
        }

        private void SpawnPlayerShip(SpaceDefinitionsManager definitions, SpaceStateManager state, SpaceSectorDef sector)
        {
            if (sector == null || !sector.TryPickSpawnPoint(definitions.Objects, out var position, out var yaw))
            {
                UnityEngine.Debug.LogError("[Space] Player ship was not created. Sector has no spawn point.");
                return;
            }

            _playerShip = SpacePlayerShip.Spawn(definitions, state, position, Quaternion.Euler(0f, yaw, 0f));
            if (_playerShip == null)
                return;

            SpaceHullDef hull = null;
            var activeHullId = ActiveHullId(state);
            if (definitions != null && !string.IsNullOrEmpty(activeHullId))
                definitions.TryGetHull(activeHullId, out hull);

            var engine = FindFittedEngine(definitions, state, out var engineSlotSize);
            SpaceSectorBodies.AttachPlayer(_session.World, _playerShip, hull, engine, engineSlotSize, position, yaw);

            var camera = Camera.main;
            if (camera == null)
            {
                UnityEngine.Debug.LogError("[Space] Player ship has no camera to orbit.");
                return;
            }

            var orbit = camera.GetComponent<SpaceOrbitCamera>();
            if (orbit == null)
                orbit = camera.gameObject.AddComponent<SpaceOrbitCamera>();

            var settings = definitions != null ? definitions.CameraSettings : null;
            var shipDistance = ReadShipCameraDistance(definitions, state);
            orbit.Follow(_playerShip, settings, SpaceCameraOrigin.Space, shipDistance);
        }

        private static string ActiveHullId(SpaceStateManager state)
        {
            var profile = state?.Profile;
            var ships = profile?.Ships;
            if (profile == null || ships == null || string.IsNullOrEmpty(profile.ActiveShipId))
                return null;

            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship != null && ship.Id == profile.ActiveShipId)
                    return ship.HullId;
            }

            return null;
        }

        private static float ReadShipCameraDistance(SpaceDefinitionsManager definitions, SpaceStateManager state)
        {
            if (definitions == null || state == null || state.Profile == null)
                return 0f;

            var hullId = ActiveHullId(state);
            return !string.IsNullOrEmpty(hullId) && definitions.TryGetHull(hullId, out var hull) ? hull.CameraDistance : 0f;
        }

        private static SpaceModuleDef FindFittedEngine(SpaceDefinitionsManager definitions, SpaceStateManager state, out SpaceSizeType slotSize)
        {
            slotSize = default;
            var ships = state?.Profile?.Ships;
            if (definitions == null || ships == null)
                return null;

            SpacePlayerShipState ship = null;
            for (var i = 0; i < ships.Count; i++)
            {
                if (ships[i] != null && ships[i].Id == state.Profile.ActiveShipId)
                {
                    ship = ships[i];
                    break;
                }
            }

            if (ship?.Slots == null)
                return null;

            for (var i = 0; i < ship.Slots.Count; i++)
            {
                var slot = ship.Slots[i];
                if (slot == null || slot.SlotType != SpaceSlotType.Engine || string.IsNullOrEmpty(slot.ModuleId))
                    continue;

                if (!definitions.TryGetModule(slot.ModuleId, out var module) || module.SlotType != SpaceSlotType.Engine)
                    continue;

                if (!module.AllowsSlotSize(slot.SizeType))
                    continue;

                slotSize = slot.SizeType;
                return module;
            }

            return null;
        }

        private void OnDestroy()
        {
            _session?.Dispose();
            _session = null;

            if (_playerShip != null)
                Destroy(_playerShip.gameObject);

            _playerShip = null;

            for (var i = 0; i < _spawnedObjects.Count; i++)
            {
                if (_spawnedObjects[i] != null)
                    Destroy(_spawnedObjects[i]);
            }

            _spawnedObjects.Clear();
        }
    }
}
