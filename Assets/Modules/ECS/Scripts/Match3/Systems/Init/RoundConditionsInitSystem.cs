using Leopotam.Ecs;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Core;
using Modules.Match3.Scripts.Interfaces;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.ECS.Scripts.Match3.Systems.Init
{
    /// <summary>
    /// Инициализация копий условий победы и поражения в мире ECS для последующей проверки.
    /// </summary>
    public class RoundConditionsInitSystem : IEcsInitSystem
    {
        private readonly EcsWorld _world = null;
        private readonly IObjectivesData _objectivesData;

        public RoundConditionsInitSystem(IObjectivesData objectivesData)
        {
            _objectivesData = objectivesData;
        }

        public void Init()
        {
            var entity = _world.NewEntity();
            entity.Get<RoundEndConditionsData>() = new RoundEndConditionsData
            {
                Victory = CloneList(_objectivesData.GetVictoryConditions()),
                Defeat = CloneList(_objectivesData.GetDefeatConditions()),
            };

            entity = _world.NewEntity();
            entity.Get<GameState>() = new GameState
            {
                IsPaused = false,
                State = RoundStateType.NA
            };

            UnityEngine.Debug.Log("[RoundConditionsInitSystem] Условия окончания раунда записаны в ECS.");
        }

        // Копия списка, чтобы не мутировать исходные коллекции из данных раунда.
        private static List<Restriction> CloneList(List<Restriction> source)
        {
            if (source == null || source.Count == 0)
                return new List<Restriction>();

            return new List<Restriction>(source);
        }
    }
}
