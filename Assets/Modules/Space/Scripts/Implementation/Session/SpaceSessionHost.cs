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

        public bool TrySetMoveTarget(Vector3 position)
        {
            var world = _session != null ? _session.World : null;
            if (world == null || !world.IsAlive())
            {
                UnityEngine.Debug.LogError("[Space] Move target was not set. The sector world is not running.");
                return false;
            }

            var count = world.GetAllEntities(ref _entityBuffer);
            for (var i = 0; i < count; i++)
            {
                var entity = _entityBuffer[i];
                if (!entity.Has<SpacePlayerHull>() || !entity.Has<SpacePosition>())
                    continue;

                ref var bodyPosition = ref entity.Get<SpacePosition>();
                entity.Get<SpaceMoveTarget>() = new SpaceMoveTarget { Position = position };
                if (!SpaceBearing.TryDegrees(bodyPosition.Value, position, out var degrees))
                    degrees = entity.Has<SpaceYaw>() ? entity.Get<SpaceYaw>().Degrees : 0f;

                entity.Get<SpaceDesiredYaw>() = new SpaceDesiredYaw { Degrees = degrees };
                UnityEngine.Debug.Log($"[Space] Player move target set to {position}.");
                return true;
            }

            UnityEngine.Debug.LogError("[Space] Move target was not set. The player hull entity is missing.");
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
            if (state != null && state.Profile != null)
                definitions.TryGetHull(state.Profile.ActiveHullId, out hull);

            SpaceSectorBodies.AttachPlayer(_session.World, _playerShip, hull, FindFittedEngine(definitions, state, hull), position, yaw);

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

        private static float ReadShipCameraDistance(SpaceDefinitionsManager definitions, SpaceStateManager state)
        {
            if (definitions == null || state == null || state.Profile == null)
                return 0f;

            return definitions.TryGetHull(state.Profile.ActiveHullId, out var hull) ? hull.CameraDistance : 0f;
        }

        private static SpaceModuleDef FindFittedEngine(SpaceDefinitionsManager definitions, SpaceStateManager state, SpaceHullDef hull)
        {
            if (definitions == null || state?.Profile?.FittedModules == null || hull?.ModuleSlotIds == null)
                return null;

            for (var i = 0; i < state.Profile.FittedModules.Count; i++)
            {
                var fitting = state.Profile.FittedModules[i];
                if (fitting == null || !definitions.TryGetModule(fitting.ModuleId, out var module))
                    continue;

                if (module.Kind != SpaceModuleKind.Engine || module.SlotId != fitting.SlotId)
                    continue;

                if (!HullHasSlot(hull, fitting.SlotId))
                    continue;

                return module;
            }

            return null;
        }

        private static bool HullHasSlot(SpaceHullDef hull, string slotId)
        {
            if (hull?.ModuleSlotIds == null)
                return false;

            for (var i = 0; i < hull.ModuleSlotIds.Count; i++)
            {
                if (hull.ModuleSlotIds[i] == slotId)
                    return true;
            }

            return false;
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
