using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Interfaces;
using System.Collections.Generic;

namespace Modules.ECS.Scripts.Match3.Systems.Match
{
    /// <summary>
    /// Система определения совпадений на игровом поле.
    /// Проверяет горизонтальные и вертикальные линии на наличие совпадений из 3+ одинаковых фишек.
    /// Создает компоненты MatchGroup для каждой найденной группы совпадений.
    /// </summary>
    public class MatchDetectionSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<CheckMatchesRequest> _checkMatchesFilter = null;
        private readonly EcsFilter<GridPosition, GemType> _gemsFilter = null;

        private readonly IGameZoneData _gameZoneData;

        public MatchDetectionSystem(IGameZoneData gameZoneData)
        {
            _gameZoneData = gameZoneData;
        }

        public void Run()
        {
            // Проверяем наличие запроса на проверку совпадений
            if (_checkMatchesFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Удаляем все запросы (обрабатываем только один раз)
            foreach (var i in _checkMatchesFilter)
            {
                _checkMatchesFilter.GetEntity(i).Del<CheckMatchesRequest>();
            }

            // Создаем карту фишек по координатам для быстрого доступа
            var gemsMap = BuildGemsMap();
            // Находим все совпадения
            var matches = FindMatches(gemsMap);

            // Создаем компоненты MatchGroup для каждого найденного совпадения
            // А также собьытия запросы на изменение очков
            foreach (var match in matches)
            {
                // Регистрируем совпадения
                var matchEntity = _world.NewEntity();
                matchEntity.Get<MatchGroup>() = match;

                // Создаем событие запроса на изменение очков 
                var scoreRequestEntity = _world.NewEntity();
                scoreRequestEntity.Get<MatchScoreRequest>() = new MatchScoreRequest
                {
                    GemType = match.GemType,
                    Count = match.Count
                };

                UnityEngine.Debug.Log($"[MatchDetectionSystem] Найдено совпадение: {match.Count} фишек типа '{match.GemType}' " +
                    $"в направлении {match.Direction} на позициях: {string.Join(", ", match.Positions)}");
            }

            if (matches.Count > 0)
            {
                UnityEngine.Debug.Log($"[MatchDetectionSystem] Всего найдено совпадений: {matches.Count}");
            }
        }

        /// <summary>
        /// Строит карту фишек по координатам для быстрого доступа
        /// </summary>
        private Dictionary<GridPosition, string> BuildGemsMap()
        {
            var gemsMap = new Dictionary<GridPosition, string>();

            foreach (var i in _gemsFilter)
            {
                ref var gridPosition = ref _gemsFilter.Get1(i);
                ref var gemType = ref _gemsFilter.Get2(i);

                gemsMap[gridPosition] = gemType.Type;
            }

            return gemsMap;
        }

        /// <summary>
        /// Находит все совпадения на игровом поле
        /// </summary>
        private List<MatchGroup> FindMatches(Dictionary<GridPosition, string> gemsMap)
        {
            var matches = new List<MatchGroup>();

            if (gemsMap.Count == 0)
            {
                return matches;
            }

            if (_gameZoneData == null)
            {
                UnityEngine.Debug.LogError($"[MatchDetectionSystem] GameZoneData is null!");
                return matches;
            }

            var mask = _gameZoneData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[MatchDetectionSystem] Mask is null!");
                return matches;
            }

            // Определяем размеры поля из маски
            int width = mask.GetLength(1);  // количество столбцов
            int height = mask.GetLength(0); // количество строк

            // Проверяем горизонтальные совпадения
            for (int y = 0; y < height; y++)
            {
                string currentType = null;
                var currentGroup = new List<GridPosition>();

                for (int x = 0; x < width; x++)
                {
                    var pos = new GridPosition { X = x, Y = y };                    
                    if (gemsMap.TryGetValue(pos, out var gemType))
                    {
                        if (gemType == currentType)
                        {
                            // Продолжаем текущую группу
                            currentGroup.Add(pos);
                        }
                        else
                        {
                            // Завершаем предыдущую группу, если она достаточно большая
                            if (currentGroup.Count >= 3)
                            {
                                var match = new MatchGroup
                                {
                                    Positions = new List<GridPosition>(currentGroup),
                                    Count = currentGroup.Count,
                                    GemType = currentType,
                                    Direction = MatchDirection.Horizontal
                                };
                                matches.Add(match);
                            }

                            // Начинаем новую группу
                            currentType = gemType;
                            currentGroup = new List<GridPosition> { pos };
                        }
                    }
                    else
                    {
                        // Пустая клетка - завершаем текущую группу
                        if (currentGroup.Count >= 3)
                        {
                            var match = new MatchGroup
                            {
                                Positions = new List<GridPosition>(currentGroup),
                                Count = currentGroup.Count,
                                GemType = currentType,
                                Direction = MatchDirection.Horizontal
                            };
                            matches.Add(match);
                        }

                        currentType = null;
                        currentGroup.Clear();
                    }
                }

                // Проверяем последнюю группу в строке
                if (currentGroup.Count >= 3)
                {
                    var match = new MatchGroup
                    {
                        Positions = new List<GridPosition>(currentGroup),
                        Count = currentGroup.Count,
                        GemType = currentType,
                        Direction = MatchDirection.Horizontal
                    };
                    matches.Add(match);
                }
            }

            // Проверяем вертикальные совпадения
            for (int x = 0; x < width; x++)
            {
                string currentType = null;
                var currentGroup = new List<GridPosition>();

                for (int y = 0; y < height; y++)
                {
                    var pos = new GridPosition { X = x, Y = y };
                    
                    if (gemsMap.TryGetValue(pos, out var gemType))
                    {
                        if (gemType == currentType)
                        {
                            // Продолжаем текущую группу
                            currentGroup.Add(pos);
                        }
                        else
                        {
                            // Завершаем предыдущую группу, если она достаточно большая
                            if (currentGroup.Count >= 3)
                            {
                                var match = new MatchGroup
                                {
                                    Positions = new List<GridPosition>(currentGroup),
                                    Count = currentGroup.Count,
                                    GemType = currentType,
                                    Direction = MatchDirection.Vertical
                                };
                                matches.Add(match);
                            }

                            // Начинаем новую группу
                            currentType = gemType;
                            currentGroup = new List<GridPosition> { pos };
                        }
                    }
                    else
                    {
                        // Пустая клетка - завершаем текущую группу
                        if (currentGroup.Count >= 3)
                        {
                            var match = new MatchGroup
                            {
                                Positions = new List<GridPosition>(currentGroup),
                                Count = currentGroup.Count,
                                GemType = currentType,
                                Direction = MatchDirection.Vertical
                            };
                            matches.Add(match);
                        }

                        currentType = null;
                        currentGroup.Clear();
                    }
                }

                // Проверяем последнюю группу в столбце
                if (currentGroup.Count >= 3)
                {
                    var match = new MatchGroup
                    {
                        Positions = new List<GridPosition>(currentGroup),
                        Count = currentGroup.Count,
                        GemType = currentType,
                        Direction = MatchDirection.Vertical
                    };
                    matches.Add(match);
                }
            }

            return matches;
        }
    }
}

