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
    // Система инициализации фоновых клеток игрового поля
    public class CellsInitSystem : IEcsInitSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;        
        private readonly IGameRoundData _gameRoundData;

        public CellsInitSystem(IGameRoundData gameRoundData)
        {
            _gameRoundData = gameRoundData;
        }

        public void Init()
        {
            UnityEngine.Debug.LogError($"_world: {_world != null}");

            if (_gameRoundData == null)
            {
                UnityEngine.Debug.LogError($"[CellsInitSystem] GameRoundData is null!");
                return;
            }

            var mask = _gameRoundData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[CellsInitSystem] Mask is null!");
                return;
            }

            // Вычисляем смещение для центрирования поля относительно камеры
            // В маске: GetLength(0) - количество строк, GetLength(1) - количество столбцов
            // В Unity координатах: X - горизонтальная ось (столбцы), Y - вертикальная ось (строки)
            int rows = mask.GetLength(0);  // количество строк в маске
            int cols = mask.GetLength(1);  // количество столбцов в маске
            int width = cols;  // ширина поля в Unity (столбцы)
            int height = rows; // высота поля в Unity (строки)
            var centeringOffset = GridPositionHelper.CalculateCenteringOffset(width, height);

            // Создаем клетки / фоновые ячейки
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
            cellObject.transform.position = GridPositionHelper.GridToWorldPosition(x, y, centeringOffset);

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

