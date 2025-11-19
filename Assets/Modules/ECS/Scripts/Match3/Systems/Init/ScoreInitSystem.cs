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
            var entity = _world.NewEntity();
            entity.Get<ScoreData>() = new ScoreData
            { 
                //TODO: ...
            };
        }
    }
}
