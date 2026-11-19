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

        public void StartSession(
            Updater updater,
            EcsSystemFactory factory,
            SpaceDefinitionsManager definitions,
            SpaceStateManager state)
        {
            if (_session != null)
                return;

            _session = new SpaceSessionController(updater, factory);
            _session.Start();
            SpawnPlayerShip(definitions, state);
        }

        private void SpawnPlayerShip(SpaceDefinitionsManager definitions, SpaceStateManager state)
        {
            _playerShip = SpacePlayerShip.Spawn(definitions, state);
            if (_playerShip == null)
                return;

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

        private void OnDestroy()
        {
            _session?.Dispose();
            _session = null;

            if (_playerShip != null)
                Destroy(_playerShip.gameObject);

            _playerShip = null;
        }
    }
}
