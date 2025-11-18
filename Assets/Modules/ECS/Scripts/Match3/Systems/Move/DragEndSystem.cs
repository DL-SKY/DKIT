using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems.Move
{
    /// <summary>
    /// Система обработки окончания перетаскивания фишек.
    /// Определяет направление движения и создает событие SwapRequest для свапа с соседней фишкой.
    /// </summary>
    public class DragEndSystem : IEcsRunSystem
    {
        private const float MIN_DRAG_DISTANCE = 0.5f;
        private const float MAX_DRAG_DISTANCE = 2.0f;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<DragState> _dragStateFilter = null;
        private readonly EcsFilter<GridPosition, GemView, Draggable> _draggableFilter = null;
        private readonly EcsFilter<MatchDestructionInProgress> _destructionInProgressFilter = null;

        public void Run()
        {
            // Проверяем, не идет ли удаление фишек (блокируем перетаскивание во время удаления)
            if (_destructionInProgressFilter.GetEntitiesCount() > 0)
            {
                // Отменяем активное перетаскивание, если оно есть
                foreach (var i in _dragStateFilter)
                {
                    _dragStateFilter.GetEntity(i).Del<DragState>();
                }
                return;
            }

            // Проверяем наличие активного перетаскивания
            if (_dragStateFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Получаем состояние перетаскивания
            ref var dragState = ref _dragStateFilter.Get1(0);
            ref var dragStateEntity = ref _dragStateFilter.GetEntity(0);

            bool dragEnded = false;
            Vector2 endScreenPosition = Vector2.zero;

            // Обработка окончания перетаскивания для мыши
            if (Input.GetMouseButtonUp(0))
            {
                dragEnded = true;
                endScreenPosition = Input.mousePosition;
            }

            // Обработка окончания перетаскивания для касаний
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    dragEnded = true;
                    endScreenPosition = touch.position;
                }
            }

            if (!dragEnded)
            {
                return;
            }

            // Определяем конечную позицию в мировых координатах
            // Используем Physics2D для работы с 2D коллайдерами
            // Для ортографической камеры Z координата должна быть 0 (или расстояние до плоскости фишек)
            float zDistance = Mathf.Abs(Camera.main.transform.position.z - dragState.StartWorldPosition.z);
            var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(endScreenPosition.x, endScreenPosition.y, zDistance));
            
            // Для определения конечной позиции используем просто мировую позицию из экранных координат
            // Z координату сохраняем из начальной позиции
            var endWorldPosition = new Vector3(worldPosition.x, worldPosition.y, dragState.StartWorldPosition.z);

            // Вычисляем вектор перетаскивания
            var dragVector = endWorldPosition - dragState.StartWorldPosition;
            var dragDistance = dragVector.magnitude;

            // Если расстояние слишком мало, отменяем перетаскивание
            if (dragDistance < MIN_DRAG_DISTANCE)
            {
                dragStateEntity.Del<DragState>();
                UnityEngine.Debug.Log("[DragEndSystem] Перетаскивание отменено: слишком малое расстояние");
                return;
            }

            // Если расстояние слишком большое, отменяем перетаскивание
            if (dragDistance > MAX_DRAG_DISTANCE)
            {
                dragStateEntity.Del<DragState>();
                UnityEngine.Debug.Log("[DragEndSystem] Перетаскивание отменено: слишком большое расстояние");
                return;
            }

            // Определяем направление свапа
            var swapDirection = GetSwapDirection(dragVector);            
            if (swapDirection == SwapDirection.None)
            {
                dragStateEntity.Del<DragState>();
                UnityEngine.Debug.Log("[DragEndSystem] Перетаскивание отменено: неопределенное направление");
                return;
            }

            // Находим соседнюю фишку в направлении свапа
            var targetEntity = FindNeighborGem(dragState.DraggedEntity, dragState.StartGridPosition, swapDirection);
            if (targetEntity.IsNull())
            {
                dragStateEntity.Del<DragState>();
                UnityEngine.Debug.Log($"[DragEndSystem] Перетаскивание отменено: не найдена соседняя фишка в направлении {swapDirection}");
                return;
            }

            // Проверяем, что обе фишки имеют компонент GemType
            if (!dragState.DraggedEntity.Has<GemType>() || !targetEntity.Has<GemType>())
            {
                dragStateEntity.Del<DragState>();
                UnityEngine.Debug.Log("[DragEndSystem] Перетаскивание отменено: одна из фишек не имеет компонента GemType");
                return;
            }

            // Проверяем, что типы фишек разные - если одинаковые, запрещаем свап
            ref var draggedGemType = ref dragState.DraggedEntity.Get<GemType>();
            ref var targetGemType = ref targetEntity.Get<GemType>();            
            if (draggedGemType.Type == targetGemType.Type)
            {
                dragStateEntity.Del<DragState>();
                UnityEngine.Debug.Log($"[DragEndSystem] Перетаскивание отменено: типы фишек одинаковые (Type: {draggedGemType.Type})");
                return;
            }

            // Создаем событие запроса свапа
            var swapRequestEntity = _world.NewEntity();
            swapRequestEntity.Get<SwapRequest>() = new SwapRequest
            {
                FromEntity = dragState.DraggedEntity,
                ToEntity = targetEntity,
                Direction = swapDirection
            };

            // Удаляем состояние перетаскивания
            dragStateEntity.Del<DragState>();

            UnityEngine.Debug.Log($"[DragEndSystem] Создан запрос свапа: ({dragState.StartGridPosition.X}, {dragState.StartGridPosition.Y}) -> {swapDirection}");
        }

        private SwapDirection GetSwapDirection(Vector3 dragVector)
        {
            // Нормализуем вектор
            var normalized = dragVector.normalized;

            // Определяем направление по наибольшей компоненте
            float absX = Mathf.Abs(normalized.x);
            float absY = Mathf.Abs(normalized.y);

            if (absX > absY)
            {
                // Горизонтальное направление
                return normalized.x > 0 ? SwapDirection.Right : SwapDirection.Left;
            }
            else
            {
                // Вертикальное направление
                return normalized.y > 0 ? SwapDirection.Up : SwapDirection.Down;
            }
        }

        private EcsEntity FindNeighborGem(EcsEntity currentEntity, GridPosition currentPosition, SwapDirection direction)
        {
            GridPosition neighborPosition = currentPosition;

            // Вычисляем позицию соседа
            switch (direction)
            {
                case SwapDirection.Up:
                    neighborPosition.Y += 1;
                    break;
                case SwapDirection.Down:
                    neighborPosition.Y -= 1;
                    break;
                case SwapDirection.Left:
                    neighborPosition.X -= 1;
                    break;
                case SwapDirection.Right:
                    neighborPosition.X += 1;
                    break;

                default:
                    return default;
            }

            // Ищем фишку на позиции соседа
            foreach (var i in _draggableFilter)
            {
                ref var gridPosition = ref _draggableFilter.Get1(i);                
                if (gridPosition.X == neighborPosition.X && gridPosition.Y == neighborPosition.Y)
                {
                    ref var entity = ref _draggableFilter.GetEntity(i);
                    // Убеждаемся, что это не та же самая фишка
                    if (!entity.Equals(currentEntity))
                    {
                        return entity;
                    }
                }
            }

            return default;
        }
    }
}

