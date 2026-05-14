using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems.Match
{
    /// <summary>
    /// Система создания новых фишек после удаления.
    /// Создает новые фишки в верхней части каждой колонки, где были удалены фишки.
    /// </summary>
    public class GemsSpawnSystem : IEcsRunSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<GameState> _gameStateFilter = null;
        private readonly EcsFilter<GemsSpawnRequest> _spawnRequestFilter = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;

        private readonly IGameZoneData _gameZoneData;
        private readonly IGemsData _gemsData;

        public GemsSpawnSystem(IGameZoneData gameZoneData, IGemsData gemsData)
        {
            _gameZoneData = gameZoneData;
            _gemsData = gemsData;
        }

        public void Run()
        {
            // Игра не активна            
            if (Helpers.GameStateHelper.IsGameStopped(_gameStateFilter))
            {
                return;
            }

            // Проверяем наличие запросов на создание фишек
            if (_spawnRequestFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Получаем смещение для центрирования поля относительно камеры
            var centeringOffset = Vector2.zero;
            foreach (var i in _offsetFilter)
            {
                ref var offset = ref _offsetFilter.Get1(i);
                centeringOffset = offset.Offset;
                break;
            }

            // Получаем маску для определения максимальной строки
            var mask = _gameZoneData?.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError("[GemsSpawnSystem] Mask is null!");
                return;
            }

            int maxRow = mask.GetLength(0) - 1; // Индекс максимальной строки (height - 1)
            int startRow = maxRow + 1; // Начальная строка для новых фишек

            // Обрабатываем все запросы
            foreach (var i in _spawnRequestFilter)
            {
                ref var spawnRequest = ref _spawnRequestFilter.Get1(i);
                ref var spawnRequestEntity = ref _spawnRequestFilter.GetEntity(i);

                if (spawnRequest.GemsCountByColumn == null || spawnRequest.GemsCountByColumn.Count == 0)
                {
                    // Удаляем пустой запрос
                    spawnRequestEntity.Del<GemsSpawnRequest>();
                    continue;
                }

                // Создаем фишки для каждой колонки
                foreach (var columnData in spawnRequest.GemsCountByColumn)
                {
                    int column = columnData.Key;
                    int gemsCount = columnData.Value;

                    // Создаем фишки друг над другом, начиная с startRow
                    for (int rowOffset = 0; rowOffset < gemsCount; rowOffset++)
                    {
                        int y = startRow + rowOffset;
                        CreateGem(column, y, centeringOffset);
                    }

                    UnityEngine.Debug.Log($"[GemsSpawnSystem] Создано {gemsCount} фишек в колонке {column}");
                }

                // Удаляем обработанный запрос
                spawnRequestEntity.Del<GemsSpawnRequest>();
            }
        }

        private void CreateGem(int x, int y, Vector2 centeringOffset)
        {
            // Генерируем случайную фишку
            var gemId = _gemsData.GetRandomGem();
            var gemDef = _definitionsManager.Gems[gemId];

            // Создаем сущность фишки через единую точку
            GemsHelper.CreateEntity(x, y, gemId, _world, centeringOffset, gemDef);
        }
    }
}

