using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Turns yaw toward the desired heading. Does not move the body and does not touch the transform.
    /// </summary>
    public sealed class SpaceRotationSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SpaceClock> _clocks = null;
        private readonly EcsFilter<SpaceYaw, SpaceDesiredYaw, SpaceTurnRate> _bodies = null;

        public void Run()
        {
            if (!SpaceClockRead.TryGetDelta(_clocks, out var deltaSeconds))
                return;

            foreach (var index in _bodies)
            {
                ref var yaw = ref _bodies.Get1(index);
                ref var desired = ref _bodies.Get2(index);
                ref var turn = ref _bodies.Get3(index);
                if (turn.DegreesPerSecond <= 0f)
                    continue;

                yaw.Degrees = Mathf.MoveTowardsAngle(yaw.Degrees, desired.Degrees, turn.DegreesPerSecond * deltaSeconds);
            }
        }
    }
}
