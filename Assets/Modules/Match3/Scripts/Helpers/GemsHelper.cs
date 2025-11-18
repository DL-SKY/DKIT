using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Implementation.Visual;
using UnityEngine;

namespace Modules.Match3.Scripts.Helpers
{
    public static class GemsHelper
    {
        private const string GEM_NAME = "Gem";


        public static void CreateEntity(int x, int y, string gemId, EcsWorld world, Vector2 centeringOffset, GemDef gemDef)
        {
            var entity = world.NewEntity();
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
        }

        public static string GenerateGemName(int x, int y)
        {
            return $"{GEM_NAME}_{x}_{y}";
        }


        private static GemVisual CreateView(int x, int y, Vector2 centeringOffset, GemDef gemDef)
        {
            var prefab = GetPrefab(gemDef.PrefabPath);
            var gemObject = Object.Instantiate(prefab);
            gemObject.Init(gemDef);
            gemObject.name = GenerateGemName(x, y);
            gemObject.transform.position = GridPositionHelper.GridToWorldPosition(x, y, centeringOffset, GridPositionHelper.GEM_POSITION_Z);

            return gemObject;
        }

        private static GemVisual GetPrefab(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                var error = $"[GemsHelper] Prefab path is null or empty!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            var prefab = Resources.Load<GemVisual>(path);
            if (prefab == null)
            {
                var error = $"[GemsHelper] Prefab \"{path}\" not loaded!";
                UnityEngine.Debug.LogError($"{error}");
                throw new System.Exception(error);
            }

            return prefab;
        }
    }
}
