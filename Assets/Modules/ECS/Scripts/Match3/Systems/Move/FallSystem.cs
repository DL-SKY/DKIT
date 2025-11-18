using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems.Move
{
    /// <summary>
    /// Система обработки падения фишек.
    /// Определяет, какие фишки должны падать вниз на свободные клетки после удаления фишек.
    /// </summary>
    public class FallSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<FallRequest> _fallRequestFilter = null;
        private readonly EcsFilter<FallInProgress> _fallInProgressFilter = null;
        private readonly EcsFilter<Match3GlobalSettingsData> _globalSettingsFilter = null;
        private readonly EcsFilter<GridPosition, GemView> _gemsFilter = null;
        private readonly EcsFilter<GridPosition, CellView> _cellsFilter = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;

        private readonly IGameZoneData _gameZoneData;

        public FallSystem(IGameZoneData gameZoneData)
        {
            _gameZoneData = gameZoneData;
        }

        public void Run()
        {
            // Если уже идет падение, не обрабатываем новые запросы
            if (_fallInProgressFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            // Проверяем наличие запросов на падение
            if (_fallRequestFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Удаляем все запросы (обрабатываем только один раз)
            foreach (var i in _fallRequestFilter)
            {
                _fallRequestFilter.GetEntity(i).Del<FallRequest>();
            }

            // Получаем смещение для центрирования
            var centeringOffset = Vector2.zero;
            foreach (var i in _offsetFilter)
            {
                ref var offset = ref _offsetFilter.Get1(i);
                centeringOffset = offset.Offset;
                break;
            }

            // Получаем длительность анимации из настроек
            var duration = 0.0f;
            foreach (var j in _globalSettingsFilter)
            {
                ref var settings = ref _globalSettingsFilter.Get1(j);
                duration = settings.GetFallAnimationDuration();
                break;
            }

            // Строим карту занятых позиций фишками
            var gemsMap = BuildGemsMap();
            // Строим карту существующих клеток
            var cellsMap = BuildCellsMap();

            // Определяем размеры поля
            var mask = _gameZoneData?.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError("[FallSystem] Mask is null!");
                return;
            }

            int width = mask.GetLength(1);
            int height = mask.GetLength(0);

            // Создаем состояние падения
            var fallInProgressEntity = _world.NewEntity();
            fallInProgressEntity.Get<FallInProgress>();

            // Обрабатываем каждый столбец отдельно
            for (int x = 0; x < width; x++)
            {
                // Множество фишек, которые уже назначены на падение
                var assignedGems = new HashSet<EcsEntity>();

                // Проходим снизу вверх по столбцу
                for (int y = 0; y < height; y++)
                {
                    var currentPos = new GridPosition { X = x, Y = y };

                    // Пропускаем, если нет клетки на этой позиции
                    if (!cellsMap.ContainsKey(currentPos))
                    {
                        continue;
                    }

                    // Если позиция свободна (нет фишки)
                    if (!gemsMap.ContainsKey(currentPos))
                    {
                        // Ищем самую верхнюю фишку выше этой позиции, которая еще не назначена
                        var gemAbove = FindHighestUnassignedGemAbove(x, y, height, gemsMap, cellsMap, assignedGems);

                        if (!gemAbove.entity.IsNull())
                        {
                            // Назначаем эту фишку на текущую свободную позицию
                            var targetPos = currentPos;
                            var (entity, currentGemPos) = gemAbove;

                            if (entity.IsNull() || !entity.Has<GemView>() || !entity.Has<GridPosition>())
                            {
                                continue;
                            }

                            ref var gemView = ref entity.Get<GemView>();
                            ref var gridPosition = ref entity.Get<GridPosition>();

                            // Вычисляем мировые позиции
                            var startWorldPosition = gemView.GemVisual != null
                                ? gemView.GemVisual.transform.position
                                : GridPositionHelper.GridToWorldPosition(currentGemPos.X, currentGemPos.Y, centeringOffset, GridPositionHelper.GEM_POSITION_Z);

                            var targetWorldPosition = GridPositionHelper.GridToWorldPosition(
                                targetPos.X,
                                targetPos.Y,
                                centeringOffset,
                                GridPositionHelper.GEM_POSITION_Z);

                            // Обновляем позицию на сетке сразу (логика игры)
                            gridPosition = targetPos;

                            // Обновляем карту для следующих итераций
                            gemsMap.Remove(currentGemPos);
                            gemsMap[targetPos] = entity;

                            // Отмечаем фишку как назначенную
                            assignedGems.Add(entity);

                            // Создаем компонент анимации падения
                            var fallAnimationEntity = _world.NewEntity();
                            fallAnimationEntity.Get<FallAnimation>() = new FallAnimation
                            {
                                Entity = entity,
                                StartPosition = startWorldPosition,
                                TargetPosition = targetWorldPosition,
                                StartGridPosition = currentGemPos,
                                TargetGridPosition = targetPos,
                                StartTime = Time.time,
                                Duration = duration
                            };

                            UnityEngine.Debug.Log($"[FallSystem] Создана анимация падения: ({currentGemPos.X}, {currentGemPos.Y}) -> ({targetPos.X}, {targetPos.Y})");
                        }
                    }
                }
            }

            UnityEngine.Debug.Log("[FallSystem] Обработка падения завершена");
        }

        /// <summary>
        /// Строит карту фишек по координатам для быстрого доступа
        /// </summary>
        private Dictionary<GridPosition, EcsEntity> BuildGemsMap()
        {
            var gemsMap = new Dictionary<GridPosition, EcsEntity>();

            foreach (var i in _gemsFilter)
            {
                ref var gridPosition = ref _gemsFilter.Get1(i);
                ref var entity = ref _gemsFilter.GetEntity(i);
                gemsMap[gridPosition] = entity;
            }

            return gemsMap;
        }

        /// <summary>
        /// Строит карту клеток по координатам для быстрого доступа
        /// </summary>
        private Dictionary<GridPosition, bool> BuildCellsMap()
        {
            var cellsMap = new Dictionary<GridPosition, bool>();

            foreach (var i in _cellsFilter)
            {
                ref var gridPosition = ref _cellsFilter.Get1(i);
                cellsMap[gridPosition] = true;
            }

            return cellsMap;
        }

        /// <summary>
        /// Находит самую верхнюю фишку выше указанной координаты Y, которая еще не назначена на падение
        /// </summary>
        private (EcsEntity entity, GridPosition position) FindHighestUnassignedGemAbove(int x, int currentY, int height, Dictionary<GridPosition, EcsEntity> gemsMap, Dictionary<GridPosition, bool> cellsMap, HashSet<EcsEntity> assignedGems)
        {
            // Ищем самую верхнюю фишку выше текущей позиции
            for (int y = currentY + 1; y < height; y++)
            {
                var pos = new GridPosition { X = x, Y = y };

                // Пропускаем, если нет клетки на этой позиции
                if (!cellsMap.ContainsKey(pos))
                {
                    continue;
                }

                // Если на этой позиции есть фишка
                if (gemsMap.TryGetValue(pos, out var gemEntity))
                {
                    // Проверяем, не назначена ли уже эта фишка на падение
                    if (!assignedGems.Contains(gemEntity))
                    {
                        // Нашли фишку, которая еще не назначена
                        return (gemEntity, pos);
                    }
                }
            }

            // Не нашли подходящую фишку
            return (default, default);
        }
    }
}

