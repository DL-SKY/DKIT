using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Spools speed along the nose toward the move target and stops where spheres would intersect.
    /// Does not change yaw and does not touch the transform.
    /// InertiaSeconds is the EVE time constant: speed closes 63% of the gap to the desired speed in that many seconds.
    /// </summary>
    public sealed class SpaceMovementSystem : IEcsRunSystem
    {
        private const float STEP_EPSILON = 0.0001f;
        private const int PUSH_PASSES = 4;

        private readonly EcsFilter<SpaceClock> _clocks = null;
        private readonly EcsFilter<SpacePosition, SpaceRadius> _bodies = null;
        private readonly EcsFilter<SpacePosition, SpaceYaw, SpaceRadius, SpaceMoveTarget, SpaceMotion> _movers = null;

        public void Run()
        {
            if (!SpaceClockRead.TryGetDelta(_clocks, out var deltaSeconds))
                return;

            foreach (var index in _movers)
            {
                ref var position = ref _movers.Get1(index);
                ref var yaw = ref _movers.Get2(index);
                ref var radius = ref _movers.Get3(index);
                ref var target = ref _movers.Get4(index);
                ref var motion = ref _movers.Get5(index);
                if (motion.MaxMetersPerSecond <= 0f)
                    continue;

                var current = position.Value;
                var forward = Quaternion.Euler(0f, yaw.Degrees, 0f) * Vector3.forward;
                ref var self = ref _movers.GetEntity(index);
                var desiredSpeed = 0f;
                if (SpaceMoveTargetRead.TryGetPose(target.Target, out var targetPosition, out var targetRadius)
                    && !target.Target.Equals(self))
                    desiredSpeed = DesiredSpeed(RemainingDistance(current, radius.Meters, targetPosition, targetRadius), motion);
                motion.MetersPerSecond = Approach(motion.MetersPerSecond, desiredSpeed, motion.InertiaSeconds, deltaSeconds);

                var step = motion.MetersPerSecond * deltaSeconds;
                if (step <= STEP_EPSILON)
                {
                    position.Value = PushOut(self, current, radius.Meters);
                    continue;
                }

                var resolved = StopAtBodies(self, current, current + forward * step, radius.Meters);
                if (Vector3.Distance(current, resolved) < step * 0.5f)
                    motion.MetersPerSecond = Approach(motion.MetersPerSecond, 0f, motion.InertiaSeconds, deltaSeconds);

                position.Value = resolved;
            }
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

        private Vector3 StopAtBodies(EcsEntity self, Vector3 from, Vector3 to, float selfRadius)
        {
            var offset = to - from;
            var distance = offset.magnitude;
            if (distance <= STEP_EPSILON)
                return PushOut(self, from, selfRadius);

            var direction = offset / distance;
            var allowed = distance;
            foreach (var index in _bodies)
            {
                if (_bodies.GetEntity(index).Equals(self))
                    continue;

                ref var otherPosition = ref _bodies.Get1(index);
                ref var otherRadius = ref _bodies.Get2(index);
                if (otherRadius.Meters <= 0f)
                    continue;

                var limit = selfRadius + otherRadius.Meters;
                if (TryHitDistance(from, direction, otherPosition.Value, limit, out var hit) && hit < allowed)
                    allowed = hit;
            }

            return PushOut(self, from + direction * allowed, selfRadius);
        }

        private Vector3 PushOut(EcsEntity self, Vector3 position, float selfRadius)
        {
            for (var pass = 0; pass < PUSH_PASSES; pass++)
            {
                var pushed = false;
                foreach (var index in _bodies)
                {
                    if (_bodies.GetEntity(index).Equals(self))
                        continue;

                    ref var otherPosition = ref _bodies.Get1(index);
                    ref var otherRadius = ref _bodies.Get2(index);
                    if (otherRadius.Meters <= 0f)
                        continue;

                    var limit = selfRadius + otherRadius.Meters;

                    var away = position - otherPosition.Value;
                    var gap = away.magnitude;
                    if (gap >= limit)
                        continue;

                    if (gap <= STEP_EPSILON)
                        away = Vector3.forward;
                    else
                        away /= gap;

                    position = otherPosition.Value + away * limit;
                    pushed = true;
                }

                if (!pushed)
                    break;
            }

            return position;
        }

        private static bool TryHitDistance(Vector3 origin, Vector3 direction, Vector3 center, float radius, out float distance)
        {
            distance = 0f;
            var offset = origin - center;
            var along = Vector3.Dot(offset, direction);
            var closest = Vector3.Dot(offset, offset) - radius * radius;
            var discriminant = along * along - closest;
            if (discriminant < 0f)
                return false;

            var root = Mathf.Sqrt(discriminant);
            var near = -along - root;
            var far = -along + root;
            if (far <= STEP_EPSILON)
                return false;

            if (near >= 0f)
            {
                distance = near;
                return true;
            }

            distance = 0f;
            return true;
        }
    }
}
