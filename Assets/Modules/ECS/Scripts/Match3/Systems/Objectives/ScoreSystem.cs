using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;

namespace Modules.ECS.Scripts.Match3.Systems.Objectives
{
    /// <summary>
    /// Система работы с учетом очков, заданий, прогресса и т.д.
    /// </summary>
    public class ScoreSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<ScoreData> _scoreFilter = null;
        private readonly EcsFilter<MatchScoreRequest> _requestFilter = null;
        private readonly EcsFilter<SwapInProgress> _swapInProgressFilter = null;
        private readonly EcsFilter<MatchDestructionInProgress> _destructionInProgressFilter = null;
        private readonly EcsFilter<FallInProgress> _fallInProgressFilter = null;

        public void Run()
        {
            //TODO: ...
            //throw new NotImplementedException();
            UnityEngine.Debug.LogError($"[ScoreSystem] Run() :: entities: {_scoreFilter.GetEntitiesCount()} " +
                $"/ requests: {_requestFilter.GetEntitiesCount()} " +
                $"/ locked: {_swapInProgressFilter.GetEntitiesCount()}/{_destructionInProgressFilter.GetEntitiesCount()}/{_fallInProgressFilter.GetEntitiesCount()}");
            //------------------------------------------------------------




            // Нет запросов
            if (_requestFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Проверяем, не идет ли свап (блок)
            if (_swapInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            // Проверяем, не идет ли удаление фишек (блок)
            if (_destructionInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            // Проверяем, не идет ли падение фишек (блок)
            if (_fallInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }



        }
    }
}
