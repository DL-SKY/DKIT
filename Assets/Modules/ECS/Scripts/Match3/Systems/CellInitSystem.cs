using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems
{
    // Система инициализации фоновых клеток игрового поля
    public class CellInitSystem : IEcsInitSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;        
        private readonly IGameRoundData _gameRoundData;

        public CellInitSystem(IGameRoundData gameRoundData)
        {
            _gameRoundData = gameRoundData;
        }

        public void Init()
        {
            if (_gameRoundData == null)
            {
                UnityEngine.Debug.LogError($"[CellInitSystem] GameRoundData is null!");
                return;
            }

            var mask = _gameRoundData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[CellInitSystem] Mask is null!");
                return;
            }

            // Вычисляем смещение для центрирования поля относительно камеры
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            var centeringOffset = GridPositionHelper.CalculateCenteringOffset(width, height);

            // Создаем клетки только там, где в маске значение отлично от нуля
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    CreateCell(x, y, centeringOffset, mask[x, y]);
        }        

        private void CreateCell(int x, int y, Vector2 centeringOffset, int maskKeyValue)
        {
            if (!_definitionsManager.CellsMap.Map.TryGetValue(maskKeyValue, out var cellId))
            {
                return;
            }

            if (!_definitionsManager.Cells.TryGetValue(cellId, out var cellDef))
            {
                UnityEngine.Debug.LogError($"[CellInitSystem] Not found Cells def \"{cellId}\"!");
                return;
            }

            var entity = _world.NewEntity();

            // Добавляем позицию
            entity.Get<CellPosition>() = new CellPosition { X = x, Y = y };

            // Создаем визуальное представление с учетом смещения для центрирования
            var prefab = GetPrefab(cellDef.PrefabPath);
            var cellObject = Object.Instantiate(prefab);
            cellObject.name = $"Cell_{x}_{y}";
            cellObject.transform.position = GridPositionHelper.GridToWorldPosition(x, y, centeringOffset);

            entity.Get<CellView>() = new CellView { GameObject = cellObject };
        }

        private GameObject GetPrefab(string path)
        {            
            if (string.IsNullOrEmpty(path))
            {
                var error = $"[CellInitSystem] Prefap path is null or empty!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                var error = $"[CellInitSystem] Prefap \"{path}\" not loaded!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            return prefab;
        }
    }
}

