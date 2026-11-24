using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Stops a moved body on the nearest sphere and pushes it back to the surface.
    /// Writes position of bodies that carry <see cref="SpaceMotion"/>. Does not change speed.
    /// A step that covered less than half of the request marks <see cref="SpaceContact"/> for this frame.
    /// </summary>
    public sealed class SpaceSeparationSystem : IEcsRunSystem
    {
        private const float STEP_EPSILON = 0.0001f;
        private const float BLOCKED_FRACTION = 0.5f;
        private const int PUSH_PASSES = 4;

        private readonly EcsFilter<SpacePosition, SpaceRadius> _bodies = null;
        private readonly EcsFilter<SpacePosition, SpaceRadius, SpaceMotion> _movers = null;

        public void Run()
        {
            foreach (var index in _movers)
            {
                ref var position = ref _movers.Get1(index);
                ref var radius = ref _movers.Get2(index);
                ref var self = ref _movers.GetEntity(index);
                var requested = 0f;
                var from = position.Value;
                if (self.Has<SpaceMotionStep>())
                {
                    ref var step = ref self.Get<SpaceMotionStep>();
                    from = step.From;
                    requested = step.RequestedMeters;
                    self.Del<SpaceMotionStep>();
                }

                var resolved = Resolve(self, from, position.Value, radius.Meters, requested);
                var blocked = requested > STEP_EPSILON
                    && Vector3.Distance(from, resolved) < requested * BLOCKED_FRACTION;
                position.Value = resolved;
                if (blocked)
                    self.Get<SpaceContact>() = new SpaceContact();
                else if (self.Has<SpaceContact>())
                    self.Del<SpaceContact>();
            }
        }

        private Vector3 Resolve(EcsEntity self, Vector3 from, Vector3 to, float selfRadius, float requested)
        {
            if (requested <= STEP_EPSILON)
                return PushOut(self, to, selfRadius);

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
