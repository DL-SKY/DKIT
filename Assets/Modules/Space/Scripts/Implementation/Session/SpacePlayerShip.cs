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

        public static Transform Spawn(SpaceDefinitionsManager definitions, SpaceStateManager state)
        {
            if (definitions == null || state == null || !state.IsReady || state.Profile == null)
            {
                UnityEngine.Debug.LogError("[Space] Player ship was not created. Profile is not ready.");
                return null;
            }

            var hullId = state.Profile.ActiveHullId;
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

            var ship = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            ship.name = OBJECT_NAME;
            UnityEngine.Debug.Log($"[Space] Player ship created from hull {hullId}.");
            return ship.transform;
        }
    }
}
