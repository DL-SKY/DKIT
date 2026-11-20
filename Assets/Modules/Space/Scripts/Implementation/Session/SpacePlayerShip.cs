using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.State;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Creates the player hull from the profile. The scene does not contain this object.
    /// </summary>
    public static class SpacePlayerShip
    {
        public const string OBJECT_NAME = "PlayerShip";

        public static Transform Spawn(SpaceDefinitionsManager definitions, SpaceStateManager state, Vector3 position, Quaternion rotation)
        {
            if (definitions == null || state == null || !state.IsReady || state.Profile == null)
            {
                UnityEngine.Debug.LogError("[Space] Player ship was not created. Profile is not ready.");
                return null;
            }

            var hullId = ActiveHullId(state.Profile);
            if (!definitions.TryGetHull(hullId, out var hull) || string.IsNullOrWhiteSpace(hull.VisualResourcesPath))
            {
                UnityEngine.Debug.LogError($"[Space] Player ship was not created. Hull '{hullId}' has no visual.");
                return null;
            }

            var prefab = Resources.Load<GameObject>(hull.VisualResourcesPath);
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[Space] Player ship visual '{hull.VisualResourcesPath}' was not found.");
                return null;
            }

            var ship = Object.Instantiate(prefab, position, rotation);
            ship.name = OBJECT_NAME;
            UnityEngine.Debug.Log($"[Space] Player ship created from hull {hullId} at {position}.");
            return ship.transform;
        }

        private static string ActiveHullId(SpaceProfile profile)
        {
            var ships = profile.Ships;
            if (ships == null || string.IsNullOrEmpty(profile.ActiveShipId))
                return null;

            for (var i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (ship != null && ship.Id == profile.ActiveShipId)
                    return ship.HullId;
            }

            return null;
        }
    }
}
