using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Heading for a circular order. Far from the ring the nose points at the target.
    /// On the ring the nose points along the stored tangent. Inside the ring it points outward.
    /// </summary>
    public static class SpaceOrbitHeading
    {
        private const float MIN_DISTANCE_SQR = 0.0001f;

        public static float Radius(float selfRadius, float targetRadius, float standoffMeters)
        {
            return Positive(selfRadius) + Positive(targetRadius) + Positive(standoffMeters);
        }

        public static bool PickClockwise(Vector3 from, Vector3 to, float currentYaw)
        {
            if (!TryOutward(from, to, out var outward))
                return true;

            var nose = Quaternion.Euler(0f, currentYaw, 0f) * Vector3.forward;
            return Vector3.Dot(nose, Clockwise(outward)) > Vector3.Dot(nose, CounterClockwise(outward));
        }

        public static bool TryDegrees(Vector3 from, Vector3 to, bool clockwise, float orbitRadius, out float degrees)
        {
            degrees = 0f;
            if (!TryOutward(from, to, out var outward))
                return false;

            var offset = from - to;
            offset.y = 0f;
            var error = offset.magnitude - Positive(orbitRadius);
            var span = orbitRadius > 1f ? orbitRadius : 1f;
            var correction = Mathf.Clamp(error / span, -1f, 1f);
            var tangent = clockwise ? Clockwise(outward) : CounterClockwise(outward);
            Vector3 aim;
            if (correction > 0f)
                aim = Vector3.Slerp(tangent, -outward, correction);
            else if (correction < 0f)
                aim = Vector3.Slerp(tangent, outward, -correction);
            else
                aim = tangent;

            if (aim.sqrMagnitude <= MIN_DISTANCE_SQR)
                return false;

            degrees = Mathf.Atan2(aim.x, aim.z) * Mathf.Rad2Deg;
            return true;
        }

        private static bool TryOutward(Vector3 from, Vector3 to, out Vector3 outward)
        {
            outward = from - to;
            outward.y = 0f;
            if (outward.sqrMagnitude <= MIN_DISTANCE_SQR)
            {
                outward = Vector3.zero;
                return false;
            }

            outward.Normalize();
            return true;
        }

        private static Vector3 Clockwise(Vector3 outward)
        {
            return new Vector3(outward.z, 0f, -outward.x);
        }

        private static Vector3 CounterClockwise(Vector3 outward)
        {
            return new Vector3(-outward.z, 0f, outward.x);
        }

        private static float Positive(float value)
        {
            return value > 0f ? value : 0f;
        }
    }
}
