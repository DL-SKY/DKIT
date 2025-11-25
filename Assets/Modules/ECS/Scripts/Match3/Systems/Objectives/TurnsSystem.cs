using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;

namespace Modules.ECS.Scripts.Match3.Systems.Objectives
{
    /// <summary>
    /// Система работы с компонентами учета ходов
    /// </summary>
    public class TurnsSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<TurnsData> _turnsFilter = null;
        private readonly EcsFilter<ChangeTurnsRequest> _requestFilter = null;
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


            foreach (var i in _turnsFilter)
            {
                ref var turnsEntity = ref _turnsFilter.GetEntity(i);

                foreach (var j in _requestFilter)
                {
                    ref var request = ref _requestFilter.Get1(j);

                    // Применяем изменения из запроса
                    turnsEntity.Get<TurnsData>().Turns += request.Delta;

                    // Удаляем запрос
                    _requestFilter.GetEntity(j).Del<ChangeTurnsRequest>();
                }

                // Создаем событие об изменении счетчика ходов
                var callbackEntity = _world.NewEntity();
                callbackEntity.Get<TurnsCallback>();

                UnityEngine.Debug.LogError($"[TurnsSystem] TurnsCount: {turnsEntity.Get<TurnsData>().Turns}");
                break;
            }            
        }
    }
}
