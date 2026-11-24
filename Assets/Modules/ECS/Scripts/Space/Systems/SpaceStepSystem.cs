using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Moves a body along its nose by the speed already stored on it. Does not change speed, yaw, or spheres.
    /// Records the segment so separation can stop it on another body. The new speed applies on the next frame.
    /// </summary>
    public sealed class SpaceStepSystem : IEcsRunSystem
    {
        private const float STEP_EPSILON = 0.0001f;

        private readonly EcsFilter<SpaceClock> _clocks = null;
        private readonly EcsFilter<SpacePosition, SpaceYaw, SpaceMotion> _bodies = null;

        public void Run()
        {
            if (!SpaceClockRead.TryGetDelta(_clocks, out var deltaSeconds))
            {
                ClearSteps();
                return;
            }

            foreach (var index in _bodies)
            {
                ref var position = ref _bodies.Get1(index);
                ref var yaw = ref _bodies.Get2(index);
                ref var motion = ref _bodies.Get3(index);
                ref var entity = ref _bodies.GetEntity(index);
                var from = position.Value;
                var step = motion.MetersPerSecond * deltaSeconds;
                if (step > STEP_EPSILON)
                {
                    var forward = Quaternion.Euler(0f, yaw.Degrees, 0f) * Vector3.forward;
                    position.Value = from + forward * step;
                    entity.Get<SpaceMotionStep>() = new SpaceMotionStep
                    {
                        From = from,
                        RequestedMeters = step
                    };
                }
                else if (entity.Has<SpaceMotionStep>())
                {
                    entity.Del<SpaceMotionStep>();
                }
            }
        }

        private void ClearSteps()
        {
            foreach (var index in _bodies)
            {
                ref var entity = ref _bodies.GetEntity(index);
                if (entity.Has<SpaceMotionStep>())
                    entity.Del<SpaceMotionStep>();
            }
        }
    }
}
