using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Creates one empty session entity when the world starts. Does not spawn ships or contacts.
    /// </summary>
    public sealed class SpaceSessionInitSystem : IEcsInitSystem
    {
        private readonly EcsWorld _world = null;

        public void Init()
        {
            if (_world == null)
            {
                UnityEngine.Debug.LogError("[Space] Session init system has no world.");
                return;
            }

            var entity = _world.NewEntity();
            entity.Get<SpaceSession>();
            entity.Get<SpaceClock>() = new SpaceClock();
            UnityEngine.Debug.Log("[Space] Session entity created.");
        }
    }
}
