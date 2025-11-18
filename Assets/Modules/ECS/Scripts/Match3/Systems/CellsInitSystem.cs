using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Definitions.Scripts.Implementation.Defs.Cells;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Implementation.Visual;
using Modules.Match3.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems
{
    /// <summary>
    /// Система инициализации фоновых клеток игрового поля
    /// </summary>
    public class CellsInitSystem : IEcsInitSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;
        private readonly EcsFilter<CenterOffsetData> _offsetFilter = null;

        private readonly IGameZoneData _gameZoneData;

        public CellsInitSystem(IGameZoneData gameZoneData)
        {
            _gameZoneData = gameZoneData;
        }

        public void Init()
        {
            if (_gameZoneData == null)
            {
                UnityEngine.Debug.LogError($"[CellsInitSystem] GameZoneData is null!");
                return;
            }

            var mask = _gameZoneData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[CellsInitSystem] Mask is null!");
                return;
            }

            // Получаем смещение для центрирования поля относительно камеры
            var centeringOffset = Vector2.zero;
            foreach (var i in _offsetFilter)
            {
                ref var offset = ref _offsetFilter.Get1(i);
                centeringOffset = offset.Offset;
            }

            // Создаем клетки / фоновые ячейки
            int width = mask.GetLength(1);
            int height = mask.GetLength(0);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    CreateCell(x, y, centeringOffset, GridPositionHelper.GetGridValueFromMatrix(x, y, mask));
        }        

        private void CreateCell(int x, int y, Vector2 centeringOffset, int maskKeyValue)
        {
            if (!_definitionsManager.CellsMap.Map.TryGetValue(maskKeyValue, out var cellId))
            {
                return;
            }

            if (!_definitionsManager.Cells.TryGetValue(cellId, out var cellDef))
            {
                UnityEngine.Debug.LogError($"[CellsInitSystem] Not found Cells def \"{cellId}\"!");
                return;
            }

            //TODO: feature - apply presets
            //...

            var entity = _world.NewEntity();
            entity.Get<GridPosition>() = new GridPosition { X = x, Y = y };         //Позиция
            entity.Get<CellView>() = new CellView                                   //Визуал
            {
                CellVisual = CreateView(cellDef, x, y, centeringOffset) 
            };
        }

        private CellVisual CreateView(CellDef cellDef, int x, int y, Vector2 centeringOffset)
        {
            var prefab = GetPrefab(cellDef.PrefabPath);
            var cellObject = Object.Instantiate(prefab);
            cellObject.name = $"Cell_{x}_{y}";
            cellObject.transform.position = GridPositionHelper.GridToWorldPosition(x, y, centeringOffset, GridPositionHelper.CELL_POSITION_Z);

            return cellObject;
        }

        private CellVisual GetPrefab(string path)
        {            
            if (string.IsNullOrEmpty(path))
            {
                var error = $"[CellsInitSystem] Prefab path is null or empty!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            var prefab = Resources.Load<CellVisual>(path);
            if (prefab == null)
            {
                var error = $"[CellsInitSystem] Prefab \"{path}\" not loaded!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            return prefab;
        }
    }
}

