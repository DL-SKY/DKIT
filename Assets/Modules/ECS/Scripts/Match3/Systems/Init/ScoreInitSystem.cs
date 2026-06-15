using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Interfaces;

namespace Modules.ECS.Scripts.Match3.Systems.Init
{
    /// <summary>
    /// Система инициализации сущностей и компонентов учета очков, заданий, прогресса и т.д.
    /// </summary>
    public class ScoreInitSystem : IEcsInitSystem
    {
        private readonly EcsWorld _world = null;

        private readonly IObjectivesData _objectivesData;

        public ScoreInitSystem(IObjectivesData objectivesData)
        {
            _objectivesData = objectivesData;
        }

        public void Init()
        {
            foreach (var score in _objectivesData.GetStartScoreValues())
            {
                var entity = _world.NewEntity();
                entity.Get<ScoreData>() = new ScoreData
                {
                    Type = score.Type,
                    Value = score.Value
                };

                UnityEngine.Debug.Log($"[ScoreInitSystem] Создан стартовый счетчик очков: {score.Type}={score.Value}");
                UnityEngine.Debug.LogError($"[ScoreInitSystem] Создан стартовый счетчик очков: {score.Type}={score.Value}");
            }
        }
    }
}
