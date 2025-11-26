using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Interfaces;

namespace Modules.ECS.Scripts.Match3.Systems.Init
{
    /// <summary>
    /// Система инициализации сущности с компонентом учета ходов
    /// </summary>
    public class TurnsInitSystem : IEcsInitSystem
    {
        private readonly EcsWorld _world = null;

        private readonly IObjectivesData _objectivesData;

        public TurnsInitSystem(IObjectivesData objectivesData)
        {
            _objectivesData = objectivesData;
        }

        public void Init()
        {
            var startTurns = _objectivesData.GetTurnsCount();
            var entity = _world.NewEntity();
            entity.Get<TurnsData>() = new TurnsData
            {
                Turns = startTurns
            };

            UnityEngine.Debug.Log($"[TurnsInitSystem] Создан счетчик Ходов: {startTurns}");
            UnityEngine.Debug.LogError($"[TurnsInitSystem] Создан счетчик Ходов: {startTurns}");
        }
    }
}
