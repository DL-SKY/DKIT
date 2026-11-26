using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Spools speed toward the route, or toward zero when the route is gone, the hull is holding position, or the last step hit a sphere.
    /// An active orbit keeps maximum speed until the order is replaced. <see cref="SpaceHoldPosition"/> is not removed.
    /// Writes <see cref="SpaceMotion.MetersPerSecond"/> only. Does not move the body.
    /// InertiaSeconds is the EVE time constant: speed closes 63% of the gap to the desired speed in that many seconds.
    /// </summary>
    public sealed class SpaceSpeedSystem : IEcsRunSystem
    {
        private const float STEP_EPSILON = 0.0001f;

        private readonly EcsFilter<SpaceClock> _clocks = null;
        private readonly EcsFilter<SpacePosition, SpaceRadius, SpaceMotion> _bodies = null;

        public void Run()
        {
            if (!SpaceClockRead.TryGetDelta(_clocks, out var deltaSeconds))
                return;

            foreach (var index in _bodies)
            {
                ref var position = ref _bodies.Get1(index);
                ref var radius = ref _bodies.Get2(index);
                ref var motion = ref _bodies.Get3(index);
                ref var entity = ref _bodies.GetEntity(index);
                if (motion.MaxMetersPerSecond <= 0f)
                    continue;

                var blocked = entity.Has<SpaceContact>();
                var holding = entity.Has<SpaceHoldPosition>();
                var hasRoute = TryRoute(entity, out var targetPosition, out var targetRadius, out var standoff, out var orbit) && !holding;
                var desiredSpeed = 0f;
                if (hasRoute && (orbit || !blocked))
                {
                    var remaining = RemainingDistance(position.Value, radius.Meters, targetPosition, targetRadius, standoff, orbit);
                    desiredSpeed = DesiredSpeed(remaining, motion, orbit);
                }

                motion.MetersPerSecond = Approach(motion.MetersPerSecond, desiredSpeed, motion.InertiaSeconds, deltaSeconds);
                if (!hasRoute && motion.MetersPerSecond * deltaSeconds <= STEP_EPSILON)
                {
                    motion.MetersPerSecond = 0f;
                    if (entity.Has<SpaceMoveTarget>())
                        entity.Del<SpaceMoveTarget>();
                }
            }
        }

        private static bool TryRoute(EcsEntity entity, out Vector3 targetPosition, out float targetRadius, out float standoff, out bool orbit)
        {
            targetPosition = default;
            targetRadius = 0f;
            standoff = 0f;
            orbit = false;
            if (!entity.Has<SpaceMoveTarget>())
                return false;

            ref var target = ref entity.Get<SpaceMoveTarget>();
            if (!SpaceMoveTargetRead.TryGetPose(target.Target, out targetPosition, out targetRadius) || target.Target.Equals(entity))
                return false;

            standoff = target.StandoffMeters > 0f ? target.StandoffMeters : 0f;
            orbit = target.Orbit;
            return true;
        }

        private static float RemainingDistance(Vector3 position, float selfRadius, Vector3 targetPosition, float targetRadius, float standoff, bool flat)
        {
            var center = flat ? FlatDistance(position, targetPosition) : Vector3.Distance(position, targetPosition);
            var gap = center - selfRadius - targetRadius - standoff;
            return gap > 0f ? gap : 0f;
        }

        private static float FlatDistance(Vector3 position, Vector3 targetPosition)
        {
            var offset = position - targetPosition;
            offset.y = 0f;
            return offset.magnitude;
        }

        private static float DesiredSpeed(float remainingDistance, SpaceMotion motion, bool orbit)
        {
            if (orbit)
                return motion.MaxMetersPerSecond;

            if (remainingDistance <= STEP_EPSILON)
                return 0f;

            var stoppingDistance = motion.MetersPerSecond * motion.InertiaSeconds;
            if (remainingDistance <= stoppingDistance)
                return 0f;

            return motion.MaxMetersPerSecond;
        }

        private static float Approach(float current, float desired, float inertiaSeconds, float deltaSeconds)
        {
            if (inertiaSeconds <= STEP_EPSILON)
                return desired;

            var blend = 1f - Mathf.Exp(-deltaSeconds / inertiaSeconds);
            var speed = current + (desired - current) * blend;
            return speed < 0f ? 0f : speed;
        }
    }
}
