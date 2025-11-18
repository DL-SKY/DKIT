using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Implementation.Visual;
using Modules.Match3.Scripts.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems.Init
{
    /// <summary>
    /// Система инициализации, отвечающая за создание и размещение гемов (драгоценных камней) на игровом поле.
    /// Создает сущности ECS для каждого гема на основе маски игровой зоны и пресетов.
    /// Предотвращает создание трех одинаковых гемов подряд (горизонтально или вертикально) для обеспечения
    /// игрового баланса. Каждый созданный гем получает компоненты: GridPosition, GemType, GemView и Draggable.
    /// </summary>
    public class GemsInitSystem : IEcsInitSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;

        private readonly IGameZoneData _gameZoneData;
        private readonly IGemsData _gemsData;

        public GemsInitSystem(IGameZoneData gameZoneData, IGemsData gemsData)
        {
            _gameZoneData = gameZoneData;
            _gemsData = gemsData;
        }

        public void Init()
        {
            if (_gameZoneData == null)
            {
                UnityEngine.Debug.LogError($"[GemsInitSystem] GameZoneData is null!");
                return;
            }

            var mask = _gameZoneData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[GemsInitSystem] Mask is null!");
                return;
            }

            var presets = _gameZoneData.GetPresets();
            if (presets == null)
            {
                UnityEngine.Debug.LogError($"[GemsInitSystem] Presets is null!");
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

            // Словарь для хранения уже созданных гемов по их позициям
            var createdGems = new Dictionary<GridPosition, string>();

            // Создаем фишки
            int width = mask.GetLength(1);
            int height = mask.GetLength(0);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    CreateGem(x, y, centeringOffset,
                        GridPositionHelper.GetGridValueFromMatrix(x, y, mask),
                        GridPositionHelper.GetGridValueFromMatrix(x, y, presets),
                        createdGems);
        }

        private void CreateGem(int x, int y, Vector2 centeringOffset, int maskKeyValue, int presetKeyValue, Dictionary<GridPosition, string> createdGems)
        {
            // Void cell on game zone
            if (!_definitionsManager.CellsMap.Map.ContainsKey(maskKeyValue))
            {
                return;
            }

            //TODO: feature - apply presets
            //...

            var excludedGems = GetExcludedGems(x, y, createdGems);            
            var gemId = (excludedGems == null || excludedGems.Count < 1) ? _gemsData.GetRandomGem() : _gemsData.GetRandomGem(excludedGems);
            var gemDef = _definitionsManager.Gems[gemId];

            var entity = _world.NewEntity();
            entity.Get<GridPosition>() = new GridPosition { X = x, Y = y };         //Позиция
            entity.Get<GemType>() = new GemType                                     //Тип
            { 
                Type = gemId
            };
            entity.Get<GemView>() = new GemView                                     //Визуал
            {
                GemVisual = CreateView(x, y, centeringOffset, gemDef)
            };
            entity.Get<Draggable>();                                                //Перетаскивание

            // Сохраняем созданный гем в словарь
            createdGems[new GridPosition { X = x, Y = y }] = gemId;
        }

        private List<string> GetExcludedGems(int x, int y, Dictionary<GridPosition, string> createdGems)
        {
            var excludedGems = new List<string>();

            // Проверка по горизонтали (слева): если два предыдущих гема одинаковые, исключаем их тип
            var leftPos1 = new GridPosition { X = x - 1, Y = y };
            var leftPos2 = new GridPosition { X = x - 2, Y = y };
            if (createdGems.TryGetValue(leftPos1, out var leftGem1)
                && createdGems.TryGetValue(leftPos2, out var leftGem2))
            {
                if (leftGem1 == leftGem2)
                {
                    excludedGems.Add(leftGem1);
                }
            }

            // Проверка по вертикали (снизу): если два предыдущих гема одинаковые, исключаем их тип
            var bottomPos1 = new GridPosition { X = x, Y = y - 1 };
            var bottomPos2 = new GridPosition { X = x, Y = y - 2 };
            if (createdGems.TryGetValue(bottomPos1, out var bottomGem1)
                && createdGems.TryGetValue(bottomPos2, out var bottomGem2))
            {
                if (bottomGem1 == bottomGem2)
                {
                    // Добавляем только если еще не добавлен (чтобы избежать дубликатов)
                    if (!excludedGems.Contains(bottomGem1))
                    {
                        excludedGems.Add(bottomGem1);
                    }
                }
            }

            return excludedGems;
        }

        private GemVisual CreateView(int x, int y, Vector2 centeringOffset, GemDef gemDef)
        { 
            var prefab = GetPrefab(gemDef.PrefabPath);
            var gemObject = Object.Instantiate(prefab);
            gemObject.Init(gemDef);
            gemObject.name = $"Gem_{x}_{y}";
            gemObject.transform.position = GridPositionHelper.GridToWorldPosition(x, y, centeringOffset, GridPositionHelper.GEM_POSITION_Z);

            return gemObject;
        }

        private GemVisual GetPrefab(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                var error = $"[GemsInitSystem] Prefab path is null or empty!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            var prefab = Resources.Load<GemVisual>(path);
            if (prefab == null)
            {
                var error = $"[GemsInitSystem] Prefab \"{path}\" not loaded!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            return prefab;
        }
    }
}
