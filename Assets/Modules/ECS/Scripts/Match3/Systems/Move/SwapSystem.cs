using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems.Move
{
    /// <summary>
    /// Система выполнения свапа фишек.
    /// Создает состояние ожидания свапа и запускает анимацию обмена позициями двух фишек.
    /// </summary>
    public class SwapSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<GameState> _gameStateFilter = null;
        private readonly EcsFilter<SwapRequest> _swapRequestFilter = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;
        private readonly EcsFilter<SwapInProgress> _swapInProgressFilter = null;
        private readonly EcsFilter<Match3GlobalSettingsData> _globalSettingsFilter = null;

        public void Run()
        {
            // Игра не активна            
            if (Helpers.GameStateHelper.IsGameStopped(_gameStateFilter))
            {
                return;
            }

            // Если уже идет свап, не обрабатываем новые запросы
            if (_swapInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            foreach (var i in _swapRequestFilter)
            {
                ref var swapRequest = ref _swapRequestFilter.Get1(i);
                ref var swapRequestEntity = ref _swapRequestFilter.GetEntity(i);

                // Проверяем, что обе сущности существуют и имеют необходимые компоненты
                if (swapRequest.FromEntity.IsNull() || swapRequest.ToEntity.IsNull())
                {
                    UnityEngine.Debug.LogWarning("[SwapSystem] Одна из сущностей в запросе свапа не существует");
                    swapRequestEntity.Del<SwapRequest>();
                    continue;
                }

                if (!swapRequest.FromEntity.Has<GridPosition>() || !swapRequest.ToEntity.Has<GridPosition>())
                {
                    UnityEngine.Debug.LogWarning("[SwapSystem] Одна из сущностей не имеет компонента GridPosition");
                    swapRequestEntity.Del<SwapRequest>();
                    continue;
                }

                if (!swapRequest.FromEntity.Has<GemView>() || !swapRequest.ToEntity.Has<GemView>())
                {
                    UnityEngine.Debug.LogWarning("[SwapSystem] Одна из сущностей не имеет компонента GemView");
                    swapRequestEntity.Del<SwapRequest>();
                    continue;
                }

                // Получаем смещение для центрирования
                var centeringOffset = Vector2.zero;
                foreach (var j in _offsetFilter)
                {
                    ref var offset = ref _offsetFilter.Get1(j);
                    centeringOffset = offset.Offset;
                    break;
                }

                // Получаем текущие позиции на сетке
                ref var fromPosition = ref swapRequest.FromEntity.Get<GridPosition>();
                ref var toPosition = ref swapRequest.ToEntity.Get<GridPosition>();

                // Получаем визуальные представления
                ref var fromView = ref swapRequest.FromEntity.Get<GemView>();
                ref var toView = ref swapRequest.ToEntity.Get<GemView>();

                // Вычисляем текущие и целевые мировые позиции
                var fromCurrentWorldPosition = fromView.GemVisual != null 
                    ? fromView.GemVisual.transform.position 
                    : GridPositionHelper.GridToWorldPosition(fromPosition.X, fromPosition.Y, centeringOffset, GridPositionHelper.GEM_POSITION_Z);
                
                var toCurrentWorldPosition = toView.GemVisual != null 
                    ? toView.GemVisual.transform.position 
                    : GridPositionHelper.GridToWorldPosition(toPosition.X, toPosition.Y, centeringOffset, GridPositionHelper.GEM_POSITION_Z);

                var fromTargetWorldPosition = GridPositionHelper.GridToWorldPosition(
                    toPosition.X,
                    toPosition.Y,
                    centeringOffset,
                    GridPositionHelper.GEM_POSITION_Z);

                var toTargetWorldPosition = GridPositionHelper.GridToWorldPosition(
                    fromPosition.X,
                    fromPosition.Y,
                    centeringOffset,
                    GridPositionHelper.GEM_POSITION_Z);

                // Меняем позиции на сетке сразу (логика игры)
                var tempPosition = fromPosition;
                fromPosition = toPosition;
                toPosition = tempPosition;

                // Создаем состояние ожидания свапа
                var swapInProgressEntity = _world.NewEntity();
                swapInProgressEntity.Get<SwapInProgress>();

                // Создаем компонент анимации свапа
                var duration = 0.0f;
                foreach (var j in _globalSettingsFilter)
                {
                    ref var settings = ref _globalSettingsFilter.Get1(j);
                    duration = settings.GetSwapAnimationDuration();
                    break;
                }
                var swapAnimationEntity = _world.NewEntity();
                swapAnimationEntity.Get<SwapAnimation>() = new SwapAnimation
                {
                    FromEntity = swapRequest.FromEntity,
                    ToEntity = swapRequest.ToEntity,
                    FromStartPosition = fromCurrentWorldPosition,
                    FromTargetPosition = fromTargetWorldPosition,
                    ToStartPosition = toCurrentWorldPosition,
                    ToTargetPosition = toTargetWorldPosition,
                    StartTime = Time.time,
                    Duration = duration
                };

                UnityEngine.Debug.Log($"[SwapSystem] Запущена анимация свапа: ({fromPosition.X}, {fromPosition.Y}) <-> ({toPosition.X}, {toPosition.Y})");

                // Удаляем событие свапа
                swapRequestEntity.Del<SwapRequest>();
            }
        }
    }
}

