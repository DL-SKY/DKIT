using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.ECS.Scripts.Match3.Helpers;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems.Match
{
    /// <summary>
    /// Система обработки запросов изменения очков после match-механики
    /// </summary>
    public class MatchScoreRequestProcessSystem : IEcsRunSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<MatchScoreRequest> _requestFilter = null;
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

                // Создаем MatchActionRequest для каждого экшена, в зависимости от фишки и величины совпадения
                var gemId = request.GemType;
                if (_definitionsManager.Gems.TryGetValue(gemId, out var def))
                    MatchActionHelper.TrySendMatchActions(def, request.Count, _world);

                // Удаляем запрос
                _requestFilter.GetEntity(i).Del<MatchScoreRequest>();
            }
        }
    }
}
