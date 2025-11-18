using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems
{
    /// <summary>
    /// Система обработки начала перетаскивания фишек.
    /// Определяет начальную фишку при нажатии мыши/касании и создает компонент DragState.
    /// </summary>
    public class DragStartSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<GridPosition, GemView, Draggable> _draggableFilter = null;
        private readonly EcsFilter<DragState> _dragStateFilter = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;
        private readonly EcsFilter<SwapInProgress> _swapInProgressFilter = null;

        public void Run()
        {
            // Проверяем, не идет ли уже перетаскивание
            if (_dragStateFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            // Проверяем, не идет ли свап (блокируем перетаскивание во время свапа)
            if (_swapInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            // Обработка начала перетаскивания для мыши
            if (Input.GetMouseButtonDown(0))
            {
                HandleDragStart(Input.mousePosition);
            }

            // Обработка начала перетаскивания для касаний
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    HandleDragStart(touch.position);
                }
            }
        }

        private void HandleDragStart(Vector2 screenPosition)
        {
            // Используем Physics2D для работы с 2D коллайдерами
            // Для ортографической камеры Z координата должна быть 0 (или расстояние до плоскости фишек)
            float zDistance = Mathf.Abs(Camera.main.transform.position.z - GridPositionHelper.GEM_POSITION_Z);
            var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistance));
            
            // Получаем смещение для центрирования
            var centeringOffset = Vector2.zero;
            foreach (var j in _offsetFilter)
            {
                ref var offset = ref _offsetFilter.Get1(j);
                centeringOffset = offset.Offset;
            }

            // Используем OverlapPointAll для получения всех коллайдеров в точке
            var hits = Physics2D.OverlapPointAll(worldPosition);
            
            EcsEntity closestEntity = default;
            float closestDistance = float.MaxValue;
            GridPosition closestGridPosition = default;

            // Проверяем все фишки и находим ближайшую к точке клика
            foreach (var i in _draggableFilter)
            {
                ref var gemView = ref _draggableFilter.Get2(i);
                ref var gridPosition = ref _draggableFilter.Get1(i);
                
                if (gemView.GemVisual == null)
                    continue;

                // Вычисляем мировую позицию фишки
                var gemWorldPosition = GridPositionHelper.GridToWorldPosition(
                    gridPosition.X, 
                    gridPosition.Y, 
                    centeringOffset, 
                    GridPositionHelper.GEM_POSITION_Z);

                // Вычисляем расстояние от точки клика до центра фишки
                var distance = Vector2.Distance(new Vector2(worldPosition.x, worldPosition.y), 
                                                new Vector2(gemWorldPosition.x, gemWorldPosition.y));

                // Проверяем, попадает ли клик в коллайдер этой фишки
                bool hitThisGem = false;
                foreach (var hit in hits)
                {
                    if (hit == null)
                        continue;
                    
                    // Проверяем, является ли коллайдер частью этой фишки
                    GameObject hitObject = hit.gameObject;
                    if (gemView.GemVisual.gameObject == hitObject)
                    {
                        hitThisGem = true;
                        break;
                    }
                    
                    // Проверяем родительскую иерархию
                    Transform parent = hitObject.transform;
                    while (parent != null)
                    {
                        if (parent.gameObject == gemView.GemVisual.gameObject)
                        {
                            hitThisGem = true;
                            break;
                        }
                        parent = parent.parent;
                    }
                    
                    if (hitThisGem)
                        break;
                }

                // Если клик попал в эту фишку и она ближе других, сохраняем её
                if (hitThisGem && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEntity = _draggableFilter.GetEntity(i);
                    closestGridPosition = gridPosition;
                }
            }

            // Если нашли фишку, создаем состояние перетаскивания
            if (!closestEntity.IsNull())
            {
                var startWorldPosition = GridPositionHelper.GridToWorldPosition(
                    closestGridPosition.X, 
                    closestGridPosition.Y, 
                    centeringOffset, 
                    GridPositionHelper.GEM_POSITION_Z);

                var dragStateEntity = _world.NewEntity();
                dragStateEntity.Get<DragState>() = new DragState
                {
                    DraggedEntity = closestEntity,
                    StartWorldPosition = startWorldPosition,
                    StartGridPosition = new GridPosition { X = closestGridPosition.X, Y = closestGridPosition.Y }
                };

                UnityEngine.Debug.Log($"[DragStartSystem] Начато перетаскивание фишки на позиции ({closestGridPosition.X}, {closestGridPosition.Y})");
            }
        }
    }
}

