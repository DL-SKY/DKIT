using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Components;
using Modules.Space.Scripts.Implementation.Session;

namespace Modules.ECS.Scripts.Space.Systems
{
    /// <summary>
    /// Writes this frame's delta onto the session entity. Does not spawn entities.
    /// </summary>
    public sealed class SpaceClockSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SpaceSession> _sessions = null;

        private readonly SpaceFrameTime _frameTime;
        private bool _missingDependencyLogged;

        public SpaceClockSystem(SpaceFrameTime frameTime)
        {
            _frameTime = frameTime;
        }

        public void Run()
        {
            if (_frameTime == null || _sessions == null)
            {
                if (!_missingDependencyLogged)
                {
                    _missingDependencyLogged = true;
                    UnityEngine.Debug.LogError("[Space] Clock system is missing frame time or the session filter.");
                }

                return;
            }

            var deltaSeconds = _frameTime.DeltaSeconds;
            foreach (var index in _sessions)
            {
                var entity = _sessions.GetEntity(index);
                ref var clock = ref entity.Get<SpaceClock>();
                clock.DeltaSeconds = deltaSeconds;
                clock.ElapsedSeconds += deltaSeconds;
            }
        }
    }
}
