using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Core;
using Modules.Restrictions.Scripts.Core;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems.Objectives
{
    /// <summary>
    /// Проверка условий победы и поражения после изменения ходов или очков (сигнал — коллбэки).
    /// Коллбэки не удаляет: очистку выполняет CallbackSystem на следующем кадре.
    /// </summary>
    public class RoundConditionsCheckSystem : IEcsRunSystem
    {
        [Inject] private readonly RestrictionsChecker _restrictionsChecker = null;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<GameState> _gameStateFilter = null;
        private readonly EcsFilter<TurnsCallback> _turnsCallbackFilter = null;
        private readonly EcsFilter<ScoreCallback> _scoreCallbackFilter = null;
        private readonly EcsFilter<RoundEndConditionsData> _conditionsFilter = null;        

        public void Run()
        {
            // Игра не активна
            if (Helpers.GameStateHelper.IsGameStopped(_gameStateFilter))
            {
                return;
            }

            // Нет коллбеков
            if (_turnsCallbackFilter.GetEntitiesCount() == 0
                && _scoreCallbackFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            foreach (var i in _conditionsFilter)
            {
                ref var gameState = ref _gameStateFilter.Get1(0);
                ref var data = ref _conditionsFilter.Get1(i);

                if (data.Victory != null
                    && data.Victory.Count > 0
                    && _restrictionsChecker.Check(data.Victory))
                {
                    UnityEngine.Debug.LogError($"WIN");
                    gameState.State = RoundStateType.Win;

                    // Создаем событие завершения раунда
                    var callbackEntity = _world.NewEntity();
                    callbackEntity.Get<RoundEndCallback>() = new RoundEndCallback
                    {
                        State = RoundStateType.Win
                    };

                    return;
                }

                if (data.Defeat != null
                    && data.Defeat.Count > 0
                    && _restrictionsChecker.Check(data.Defeat))
                {
                    UnityEngine.Debug.LogError($"LOSE");
                    gameState.State = RoundStateType.Lose;

                    // Создаем событие завершения раунда
                    var callbackEntity = _world.NewEntity();
                    callbackEntity.Get<RoundEndCallback>() = new RoundEndCallback
                    {
                        State = RoundStateType.Lose
                    };

                    return;
                }
            }
        }
    }
}
