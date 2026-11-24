using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Spools speed toward the route, or toward zero when the route is gone or the last step hit a sphere.
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
                var hasRoute = TryRoute(entity, out var targetPosition, out var targetRadius);
                var desiredSpeed = 0f;
                if (hasRoute && !blocked)
                    desiredSpeed = DesiredSpeed(RemainingDistance(position.Value, radius.Meters, targetPosition, targetRadius), motion);

                motion.MetersPerSecond = Approach(motion.MetersPerSecond, desiredSpeed, motion.InertiaSeconds, deltaSeconds);
                if (!hasRoute && motion.MetersPerSecond * deltaSeconds <= STEP_EPSILON)
                {
                    motion.MetersPerSecond = 0f;
                    if (entity.Has<SpaceMoveTarget>())
                        entity.Del<SpaceMoveTarget>();
                }
            }
        }

        private static bool TryRoute(EcsEntity entity, out Vector3 targetPosition, out float targetRadius)
        {
            targetPosition = default;
            targetRadius = 0f;
            if (!entity.Has<SpaceMoveTarget>())
                return false;

            ref var target = ref entity.Get<SpaceMoveTarget>();
            return SpaceMoveTargetRead.TryGetPose(target.Target, out targetPosition, out targetRadius)
                && !target.Target.Equals(entity);
        }

        private static float RemainingDistance(Vector3 position, float selfRadius, Vector3 targetPosition, float targetRadius)
        {
            var gap = Vector3.Distance(position, targetPosition) - selfRadius - targetRadius;
            return gap > 0f ? gap : 0f;
        }

        private static float DesiredSpeed(float remainingDistance, SpaceMotion motion)
        {
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
