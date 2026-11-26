using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Points the desired yaw at the move target, or along an orbit ring when the order says so.
    /// Does not turn the hull and does not change speed.
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
                if (!SpaceMoveTargetRead.TryGetPose(target.Target, out var targetPosition, out var targetRadius))
                    continue;

                if (target.Orbit)
                {
                    ref var entity = ref _bodies.GetEntity(index);
                    var selfRadius = entity.Has<SpaceRadius>() ? entity.Get<SpaceRadius>().Meters : 0f;
                    var orbitRadius = SpaceOrbitHeading.Radius(selfRadius, targetRadius, target.StandoffMeters);
                    if (SpaceOrbitHeading.TryDegrees(position.Value, targetPosition, target.OrbitClockwise, orbitRadius, out var orbitYaw))
                        desired.Degrees = orbitYaw;
                    continue;
                }

                if (SpaceBearing.TryDegrees(position.Value, targetPosition, out var degrees))
                    desired.Degrees = degrees;
            }
        }
    }
}
