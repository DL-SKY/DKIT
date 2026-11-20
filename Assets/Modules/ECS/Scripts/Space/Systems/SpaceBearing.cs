using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Flat bearing from one point to another. Zero looks along +Z, positive turns toward +X.
    /// </summary>
    public static class SpaceBearing
    {
        private const float MIN_DISTANCE_SQR = 0.0001f;

        public static bool TryDegrees(Vector3 from, Vector3 to, out float degrees)
        {
            var offset = to - from;
            offset.y = 0f;
            if (offset.sqrMagnitude <= MIN_DISTANCE_SQR)
            {
                degrees = 0f;
                return false;
            }

            degrees = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            return true;
        }
    }
}
