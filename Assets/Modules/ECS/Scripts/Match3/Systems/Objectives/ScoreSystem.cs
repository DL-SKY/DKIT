using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.ECS.Scripts.Match3.Components;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems.Objectives
{
    /// <summary>
    /// Система учета очков / буста / прогресса
    /// </summary>
    public class ScoreSystem : IEcsRunSystem
    {
        //[Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<ScoreData> _scoreFilter = null;
        private readonly EcsFilter<ChangeScoreRequest> _requestFilter = null;
        //private readonly EcsFilter<SwapInProgress> _swapInProgressFilter = null;
        //private readonly EcsFilter<MatchDestructionInProgress> _destructionInProgressFilter = null;
        //private readonly EcsFilter<FallInProgress> _fallInProgressFilter = null;

        public void Run()
        {
            // Нет запросов
            if (_requestFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            //// Проверяем, не идет ли свап (блок)
            //if (_swapInProgressFilter.GetEntitiesCount() > 0)
            //{
            //    return;
            //}

            //// Проверяем, не идет ли удаление фишек (блок)
            //if (_destructionInProgressFilter.GetEntitiesCount() > 0)
            //{
            //    return;
            //}

            //// Проверяем, не идет ли падение фишек (блок)
            //if (_fallInProgressFilter.GetEntitiesCount() > 0)
            //{
            //    return;
            //}

            // Обработка запросов
            foreach (var i in _requestFilter)
            {
                ref var request = ref _requestFilter.Get1(i);

                var applied = false;
                foreach (var j in _scoreFilter)
                {
                    ref var score = ref _scoreFilter.Get1(j);
                    if (score.Type == request.Type)
                    {
                        score.Value += request.Delta;
                        applied = true;
                        break;
                    }
                }

                if (!applied)
                {
                    var entity = _world.NewEntity();
                    entity.Get<ScoreData>() = new ScoreData { 
                        Type = request.Type,
                        Value = request.Delta
                    };
                }

                // Создаем событие об изменении счетчика очков
                var callbackEntity = _world.NewEntity();
                callbackEntity.Get<ScoreCallback>() = new ScoreCallback { 
                    Type = request.Type
                };

                // Удаляем запрос
                _requestFilter.GetEntity(i).Del<ChangeScoreRequest>();
            }
        }
    }
}
