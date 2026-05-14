using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;

namespace Modules.ECS.Scripts.Match3.Systems.Events
{
    /// <summary>
    /// Система работы с компонентами-колбэками / callback
    /// </summary>
    public class CallbackSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<TurnsCallback> _turnCallbackFilter = null;
        private readonly EcsFilter<ScoreCallback> _scoreCallbackFilter = null;
        private readonly EcsFilter<RoundEndCallback> _roundEndCallbackFilter = null;

        public void Run()
        {
            TurnsCallbackProcess();
            ScoreCallbackProcess();
            RoundEndCallbackProcess();
        }

        private void ClearCallbacks<T>(EcsFilter<T> filter) where T : struct
        {
            // Удаляем коллбэки
            foreach (var i in filter)
                filter.GetEntity(i).Del<T>();
        }

        private void TurnsCallbackProcess()
        {
            if (_turnCallbackFilter.GetEntitiesCount() == 0)
                return;

            ClearCallbacks(_turnCallbackFilter);
        }

        private void ScoreCallbackProcess()
        {
            if (_scoreCallbackFilter.GetEntitiesCount() == 0)
                return;

            ClearCallbacks(_scoreCallbackFilter);
        }

        private void RoundEndCallbackProcess()
        {
            if (_roundEndCallbackFilter.GetEntitiesCount() == 0)
                return;

            ClearCallbacks(_roundEndCallbackFilter);
        }
    }
}
