using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Copies the entity pose onto its transform. Runs after rotation and movement.
    /// </summary>
    public sealed class SpacePoseSyncSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SpacePosition, SpaceYaw, SpaceView> _views = null;

        public void Run()
        {
            if (_views == null)
                return;

            foreach (var index in _views)
            {
                ref var position = ref _views.Get1(index);
                ref var yaw = ref _views.Get2(index);
                ref var view = ref _views.Get3(index);
                if (view.Transform == null)
                    continue;

                view.Transform.SetPositionAndRotation(position.Value, Quaternion.Euler(0f, yaw.Degrees, 0f));
            }
        }
    }
}
