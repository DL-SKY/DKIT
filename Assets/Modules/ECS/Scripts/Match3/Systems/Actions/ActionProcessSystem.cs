using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.ECS.Scripts.Match3.Components;
using System;
using System.Collections.Generic;

namespace Modules.ECS.Scripts.Match3.Systems.Actions
{
    /// <summary>
    /// Система обработки MatchActionRequest
    /// </summary>
    public class ActionProcessSystem : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        private readonly EcsFilter<MatchActionRequest> _actionsFilter = null;

        private Dictionary<MatchActionType, IActionApplier> _appliers = new Dictionary<MatchActionType, IActionApplier>();

        public void Run()
        {
            // Нет запросов
            if (_actionsFilter.GetEntitiesCount() == 0)
            {
                return;
            }

            // Обработка запросов
            foreach (var i in _actionsFilter)
            {
                ref var actionData = ref _actionsFilter.Get1(i);
                GetApplier(actionData.Action.Type).Apply(actionData.Action);

                // Удаляем запрос
                _actionsFilter.GetEntity(i).Del<MatchActionRequest>();
            }
        }

        private IActionApplier GetApplier(MatchActionType actionType)
        {
            if (!_appliers.ContainsKey(actionType))
                _appliers.Add(actionType, CreateApplier(actionType));

            return _appliers[actionType];
        }

        private IActionApplier CreateApplier(MatchActionType actionType)
        {
            return actionType switch
            {
                MatchActionType.ScoreChange => new ScoreChangeApplier().Init(_world),
                MatchActionType.TurnsChange => new TurnsChangeApplier().Init(_world),

                _ => throw new NotImplementedException()
            };
        }
    }
}
