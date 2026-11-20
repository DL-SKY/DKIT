using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Points the desired yaw at the move target. Does not turn the hull and does not change speed.
    /// </summary>
    public sealed class SpaceFaceMoveTargetSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SpacePosition, SpaceMoveTarget, SpaceDesiredYaw> _bodies = null;

        public void Run()
        {
            if (_bodies == null)
                return;

            foreach (var index in _bodies)
            {
                ref var position = ref _bodies.Get1(index);
                ref var target = ref _bodies.Get2(index);
                ref var desired = ref _bodies.Get3(index);
                if (SpaceBearing.TryDegrees(position.Value, target.Position, out var degrees))
                    desired.Degrees = degrees;
            }
        }
    }
}
