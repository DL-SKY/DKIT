using Leopotam.Ecs;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.ECS.Scripts.Examples
{
    // Система инициализации игрового поля
    public class GridInitSystem : IEcsInitSystem
    {
        private readonly EcsWorld _world = null;
        private const int GridWidth = 8;
        private const int GridHeight = 8;
        private readonly Color[] _gemColors = new Color[]
        {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan
        };

        public void Init()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    CreateGem(x, y);
                }
            }
        }

        private void CreateGem(int x, int y)
        {
            var entity = _world.NewEntity();

            int gemType = GetRandomGemType(x, y);

            entity.Get<GridPosition>() = new GridPosition { X = x, Y = y };
            entity.Get<GemType>() = new GemType
            {
                TypeId = gemType,
                Color = _gemColors[gemType]
            };
            entity.Get<Movable>();
            entity.Get<Draggable>(); // Добавляем возможность перетаскивания

            CreateGemView(entity, x, y, _gemColors[gemType]);
        }

        private int GetRandomGemType(int x, int y)
        {
            return Random.Range(0, _gemColors.Length);
        }

        private void CreateGemView(EcsEntity entity, int x, int y, Color color)
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = $"Gem_{x}_{y}";
            gameObject.transform.position = new Vector3(x, y, 0);
            gameObject.GetComponent<Renderer>().material.color = color;

            // Добавляем коллайдер для обнаружения кликов
            if (gameObject.GetComponent<BoxCollider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }

            entity.Get<GemView>() = new GemView { GameObject = gameObject };
        }

        //private readonly EcsWorld _world = null;
        //private const int GridWidth = 8;
        //private const int GridHeight = 8;
        //private readonly Color[] _gemColors = new Color[]
        //{
        //    Color.red,
        //    Color.green,
        //    Color.blue,
        //    Color.yellow,
        //    Color.magenta
        //};

        //public void Init()
        //{
        //    for (int x = 0; x < GridWidth; x++)
        //    {
        //        for (int y = 0; y < GridHeight; y++)
        //        {
        //            CreateGem(x, y);
        //        }
        //    }

        //    // Запускаем первоначальную проверку матчей
        //    _world.NewEntity().Get<CheckMatches>();
        //}

        //private void CreateGem(int x, int y)
        //{
        //    var entity = _world.NewEntity();

        //    // Случайный тип фишки (избегаем матчей при спавне)
        //    int gemType = GetRandomGemType(x, y);

        //    entity.Get<GridPosition>() = new GridPosition { X = x, Y = y };
        //    entity.Get<GemType>() = new GemType
        //    {
        //        TypeId = gemType,
        //        Color = _gemColors[gemType]
        //    };
        //    entity.Get<Movable>();

        //    // Создаем визуальное представление
        //    CreateGemView(entity, x, y, _gemColors[gemType]);
        //}

        //private int GetRandomGemType(int x, int y)
        //{
        //    // Простая логика для избежания начальных матчей
        //    int gemType;
        //    int attempts = 0;

        //    do
        //    {
        //        gemType = Random.Range(0, _gemColors.Length);
        //        attempts++;
        //    }
        //    while (HasInitialMatch(x, y, gemType) && attempts < 10);

        //    return gemType;
        //}

        //private bool HasInitialMatch(int x, int y, int gemType)
        //{
        //    // Проверяем соседей слева
        //    if (x >= 2)
        //    {
        //        // Логика проверки будет в системе матчей
        //    }
        //    return false;
        //}

        //private void CreateGemView(EcsEntity entity, int x, int y, Color color)
        //{
        //    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    gameObject.name = $"Gem_{x}_{y}";
        //    gameObject.transform.position = new Vector3(x, y, 0);
        //    gameObject.GetComponent<Renderer>().material.color = color;

        //    entity.Get<GemView>() = new GemView { GameObject = gameObject };
        //}
    }

    //--------------------------------------------------

    // Система выбора фишек
    public class SelectionSystem : IEcsRunSystem
    {
        private readonly EcsFilter<GridPosition, GemView, Movable> _gemsFilter = null;
        private readonly EcsWorld _world = null;
        private static EcsEntity _selectedEntity;

        public void Run()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    foreach (var i in _gemsFilter)
                    {
                        ref var gemView = ref _gemsFilter.Get2(i);

                        if (gemView.GameObject == hit.transform.gameObject)
                        {
                            ref var entity = ref _gemsFilter.GetEntity(i);

                            if (_selectedEntity.IsNull())
                            {
                                // Первый выбор
                                _selectedEntity = entity;
                                entity.Get<Selected>();
                                UpdateSelectionVisual(entity, true);
                            }
                            else
                            {
                                // Второй выбор - проверяем соседство и создаем событие свапа
                                if (IsAdjacent(_selectedEntity, entity))
                                {
                                    _world.NewEntity().Get<SwapEvent>() = new SwapEvent
                                    {
                                        FromEntity = _selectedEntity,
                                        ToEntity = entity
                                    };
                                }

                                // Снимаем выделение
                                if (_selectedEntity.Has<Selected>())
                                {
                                    _selectedEntity.Del<Selected>();
                                    UpdateSelectionVisual(_selectedEntity, false);
                                }
                                _selectedEntity = default;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private bool IsAdjacent(EcsEntity a, EcsEntity b)
        {
            ref var posA = ref a.Get<GridPosition>();
            ref var posB = ref b.Get<GridPosition>();

            return (Mathf.Abs(posA.X - posB.X) == 1 && posA.Y == posB.Y) ||
                   (Mathf.Abs(posA.Y - posB.Y) == 1 && posA.X == posB.X);
        }

        private void UpdateSelectionVisual(EcsEntity entity, bool selected)
        {
            if (entity.Has<GemView>())
            {
                ref var gemView = ref entity.Get<GemView>();
                var renderer = gemView.GameObject.GetComponent<Renderer>();
                renderer.material.color = selected ?
                    Color.white :
                    entity.Get<GemType>().Color;
            }
        }
    }

    //-------------------------------------------------------------------

    // Система свапа фишек
    public class SwapSystem : IEcsRunSystem
    {
        private readonly EcsFilter<SwapEvent> _swapFilter = null;

        public void Run()
        {
            foreach (var i in _swapFilter)
            {
                ref var swapEvent = ref _swapFilter.Get1(i);

                SwapGems(swapEvent.FromEntity, swapEvent.ToEntity);

                // Запрашиваем проверку матчей
                swapEvent.FromEntity.Get<CheckMatches>();

                _swapFilter.GetEntity(i).Destroy();
            }
        }

        private void SwapGems(EcsEntity a, EcsEntity b)
        {
            ref var posA = ref a.Get<GridPosition>();
            ref var posB = ref b.Get<GridPosition>();

            // Меняем позиции местами
            (posA.X, posB.X) = (posB.X, posA.X);
            (posA.Y, posB.Y) = (posB.Y, posA.Y);

            // Обновляем визуальное положение
            UpdateViewPosition(a);
            UpdateViewPosition(b);
        }

        private void UpdateViewPosition(EcsEntity entity)
        {
            if (entity.Has<GridPosition>() && entity.Has<GemView>())
            {
                ref var position = ref entity.Get<GridPosition>();
                ref var view = ref entity.Get<GemView>();
                view.GameObject.transform.position = new Vector3(position.X, position.Y, 0);
            }
        }
    }

    //------------------------------------------------------------------

    // Система проверки матчей
    public class MatchCheckSystem : IEcsRunSystem
    {
        private readonly EcsFilter<GridPosition, GemType> _gemsFilter = null;
        private readonly EcsWorld _world = null;

        public void Run()
        {
            foreach (var i in _gemsFilter)
            {
                if (_gemsFilter.GetEntity(i).Has<CheckMatches>())
                {
                    ref var position = ref _gemsFilter.Get1(i);
                    ref var gemType = ref _gemsFilter.Get2(i);

                    var matches = FindMatchesAt(position.X, position.Y, gemType.TypeId);

                    if (matches.Count >= 3)
                    {
                        // Помечаем все совпавшие фишки на уничтожение
                        foreach (var matchPos in matches)
                        {
                            var matchEntity = FindGemAt(matchPos.x, matchPos.y);
                            if (!matchEntity.IsNull())
                            {
                                matchEntity.Get<DestroyTag>();
                            }
                        }
                    }

                    _gemsFilter.GetEntity(i).Del<CheckMatches>();
                }
            }
        }

        private List<Vector2Int> FindMatchesAt(int x, int y, int typeId)
        {
            var matches = new List<Vector2Int>();

            // Проверка по горизонтали
            var horizontalMatches = new List<Vector2Int> { new Vector2Int(x, y) };

            // Проверяем влево
            for (int i = x - 1; i >= 0; i--)
            {
                if (IsSameType(i, y, typeId))
                    horizontalMatches.Add(new Vector2Int(i, y));
                else break;
            }

            // Проверяем вправо
            for (int i = x + 1; i < 8; i++)
            {
                if (IsSameType(i, y, typeId))
                    horizontalMatches.Add(new Vector2Int(i, y));
                else break;
            }

            if (horizontalMatches.Count >= 3)
                matches.AddRange(horizontalMatches);

            // Проверка по вертикали
            var verticalMatches = new List<Vector2Int> { new Vector2Int(x, y) };

            // Проверяем вниз
            for (int j = y - 1; j >= 0; j--)
            {
                if (IsSameType(x, j, typeId))
                    verticalMatches.Add(new Vector2Int(x, j));
                else break;
            }

            // Проверяем вверх
            for (int j = y + 1; j < 8; j++)
            {
                if (IsSameType(x, j, typeId))
                    verticalMatches.Add(new Vector2Int(x, j));
                else break;
            }

            if (verticalMatches.Count >= 3)
                matches.AddRange(verticalMatches);

            return matches;
        }

        private bool IsSameType(int x, int y, int typeId)
        {
            var entity = FindGemAt(x, y);
            return !entity.IsNull() && entity.Get<GemType>().TypeId == typeId;
        }

        private EcsEntity FindGemAt(int x, int y)
        {
            foreach (var i in _gemsFilter)
            {
                ref var position = ref _gemsFilter.Get1(i);
                if (position.X == x && position.Y == y)
                    return _gemsFilter.GetEntity(i);
            }
            return default;
        }
    }

    //----------------------------------------------------------------

    // Система уничтожения фишек
    public class DestroySystem : IEcsRunSystem
    {
        private readonly EcsFilter<DestroyTag, GridPosition, GemView> _destroyFilter = null;
        private readonly EcsWorld _world = null;

        public void Run()
        {
            foreach (var i in _destroyFilter)
            {
                ref var position = ref _destroyFilter.Get2(i);
                ref var view = ref _destroyFilter.Get3(i);

                UnityEngine.Debug.LogError($"Destroyed gem at ({position.X}, {position.Y})");

                // Уничтожаем GameObject
                Object.Destroy(view.GameObject);

                // Уничтожаем сущность
                _destroyFilter.GetEntity(i).Destroy();
            }
        }
    }

    //-------------------------------------------------------------------

    // Система начала перетаскивания
    public class DragStartSystem : IEcsRunSystem
    {
        private readonly EcsFilter<GridPosition, GemView, Draggable> _draggableFilter = null;
        private readonly EcsWorld _world = null;
        private bool _isDragging = false;
        private EcsEntity _draggedEntity;
        private Vector2 _dragStartPosition;

        public void Run()
        {
            if (!_isDragging && Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    foreach (var i in _draggableFilter)
                    {
                        ref var gemView = ref _draggableFilter.Get2(i);

                        if (gemView.GameObject == hit.transform.gameObject)
                        {
                            _draggedEntity = _draggableFilter.GetEntity(i);
                            _dragStartPosition = Input.mousePosition;
                            _isDragging = true;

                            // Визуальная обратная связь - подсветка
                            SetGemHighlight(_draggedEntity, true);
                            break;
                        }
                    }
                }
            }
        }

        private void SetGemHighlight(EcsEntity entity, bool highlight)
        {
            if (entity.Has<GemView>())
            {
                ref var gemView = ref entity.Get<GemView>();
                var renderer = gemView.GameObject.GetComponent<Renderer>();
                var originalColor = entity.Get<GemType>().Color;
                renderer.material.color = highlight ?
                    Color.Lerp(originalColor, Color.white, 0.3f) :
                    originalColor;
            }
        }
    }

    //----------------------------------------------------------------

    // Система завершения перетаскивания и определения направления
    public class DragEndSystem : IEcsRunSystem
    {
        private readonly EcsFilter<GridPosition, GemView, Draggable> _draggableFilter = null;
        private readonly EcsWorld _world = null;

        // Переносим состояние из DragStartSystem
        private bool _isDragging = false;
        private EcsEntity _draggedEntity;
        private Vector3 _dragStartPosition; // Изменяем на Vector3

        public void Run()
        {
            // Обработка начала перетаскивания (переносим логику сюда для целостности)
            if (!_isDragging && Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    foreach (var i in _draggableFilter)
                    {
                        ref var gemView = ref _draggableFilter.Get2(i);

                        if (gemView.GameObject == hit.transform.gameObject)
                        {
                            _draggedEntity = _draggableFilter.GetEntity(i);
                            _dragStartPosition = Input.mousePosition; // Vector3
                            _isDragging = true;

                            SetGemHighlight(_draggedEntity, true);
                            UnityEngine.Debug.LogError($"Started dragging gem at position: {_dragStartPosition}");
                            break;
                        }
                    }
                }
            }

            // Обработка завершения перетаскивания
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                var dragEndPosition = Input.mousePosition; // Vector3
                var dragVector = dragEndPosition - _dragStartPosition; // Теперь оба Vector3

                // Определяем направление свапа
                var swapDirection = GetSwapDirection(dragVector);
                var swapTarget = GetSwapTarget(_draggedEntity, swapDirection);

                if (!swapTarget.IsNull())
                {
                    // Создаем событие свапа
                    _world.NewEntity().Get<SwapEvent>() = new SwapEvent
                    {
                        FromEntity = _draggedEntity,
                        ToEntity = swapTarget
                    };
                    UnityEngine.Debug.LogError($"Swap initiated with direction: {swapDirection}");
                }
                else
                {
                    UnityEngine.Debug.LogError("No valid swap target found");
                }

                // Сбрасываем состояние перетаскивания
                SetGemHighlight(_draggedEntity, false);
                _isDragging = false;
                _draggedEntity = default;
            }
        }

        private Vector2Int GetSwapDirection(Vector3 dragVector)
        {
            // Минимальная дистанция для регистрации свапа
            float minDragDistance = 50f;

            if (dragVector.magnitude < minDragDistance)
                return Vector2Int.zero;

            // Определяем основное направление
            if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
            {
                return dragVector.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                return dragVector.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        private EcsEntity GetSwapTarget(EcsEntity draggedEntity, Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
                return default;

            ref var draggedPos = ref draggedEntity.Get<GridPosition>();
            int targetX = draggedPos.X + direction.x;
            int targetY = draggedPos.Y + direction.y;

            // Проверяем границы сетки
            if (targetX < 0 || targetX >= 8 || targetY < 0 || targetY >= 8)
                return default;

            // Ищем сущность в целевой позиции
            foreach (var i in _draggableFilter)
            {
                ref var pos = ref _draggableFilter.Get1(i);
                if (pos.X == targetX && pos.Y == targetY)
                {
                    return _draggableFilter.GetEntity(i);
                }
            }

            return default;
        }

        private void SetGemHighlight(EcsEntity entity, bool highlight)
        {
            if (entity.Has<GemView>())
            {
                ref var gemView = ref entity.Get<GemView>();
                var renderer = gemView.GameObject.GetComponent<Renderer>();
                var originalColor = entity.Get<GemType>().Color;
                renderer.material.color = highlight ?
                    Color.Lerp(originalColor, Color.white, 0.3f) :
                    originalColor;
            }
        }
    }

    //----------------------------------------------------------------

    // Система анимации перетаскивания (опционально)
    public class DragAnimationSystem : IEcsRunSystem
    {
        private readonly EcsFilter<GridPosition, GemView> _gemsFilter = null;
        private bool _isDragging = false;
        private EcsEntity _draggedEntity;
        private Vector3 _dragOffset;

        public void Run()
        {
            if (_isDragging)
            {
                // Получаем позицию в мировых координатах
                var mousePos = Input.mousePosition;
                mousePos.z = 10f; // Расстояние от камеры
                var worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                if (!_draggedEntity.IsNull() && _draggedEntity.Has<GemView>())
                {
                    ref var gemView = ref _draggedEntity.Get<GemView>();
                    gemView.GameObject.transform.position = worldPos + _dragOffset;
                }
            }
        }
    }
}
