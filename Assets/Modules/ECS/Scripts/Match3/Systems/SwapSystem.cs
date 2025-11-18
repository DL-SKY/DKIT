using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems
{
    /// <summary>
    /// Система выполнения свапа фишек.
    /// Меняет позиции двух фишек на сетке и обновляет их визуальное представление.
    /// </summary>
    public class SwapSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SwapRequest> _swapRequestFilter = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;

        public void Run()
        {
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
                }

                // Получаем позиции
                ref var fromPosition = ref swapRequest.FromEntity.Get<GridPosition>();
                ref var toPosition = ref swapRequest.ToEntity.Get<GridPosition>();

                // Меняем позиции на сетке
                var tempPosition = fromPosition;
                fromPosition = toPosition;
                toPosition = tempPosition;

                // Обновляем визуальное представление
                ref var fromView = ref swapRequest.FromEntity.Get<GemView>();
                ref var toView = ref swapRequest.ToEntity.Get<GemView>();

                if (fromView.GemVisual != null)
                {
                    var newWorldPosition = GridPositionHelper.GridToWorldPosition(
                        fromPosition.X,
                        fromPosition.Y,
                        centeringOffset,
                        GridPositionHelper.GEM_POSITION_Z);
                    fromView.GemVisual.transform.position = newWorldPosition;
                }

                if (toView.GemVisual != null)
                {
                    var newWorldPosition = GridPositionHelper.GridToWorldPosition(
                        toPosition.X,
                        toPosition.Y,
                        centeringOffset,
                        GridPositionHelper.GEM_POSITION_Z);
                    toView.GemVisual.transform.position = newWorldPosition;
                }

                UnityEngine.Debug.Log($"[SwapSystem] Выполнен свап: ({fromPosition.X}, {fromPosition.Y}) <-> ({toPosition.X}, {toPosition.Y})");

                // Удаляем событие свапа
                swapRequestEntity.Del<SwapRequest>();
            }
        }
    }
}

