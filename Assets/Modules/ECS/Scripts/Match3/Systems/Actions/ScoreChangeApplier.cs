using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.ECS.Scripts.Match3.Components;
using System;

namespace Modules.ECS.Scripts.Match3.Systems.Actions
{
    public class ScoreChangeApplier : IActionApplier
    {
        private EcsWorld _world;

        public IActionApplier Init(EcsWorld world)
        {
            _world = world;
            return this;
        }

        public void Apply(MatchAction action)
        {
            if (action.Type != MatchActionType.ScoreChange)
                return;

            //ChangeScoreRequest
            var type = ScoreType.NA;
            if (Enum.TryParse<ScoreType>(action.StringParameter1, out var parseType))
                type = parseType;
            var delta = action.IntParameter1;
            var entity = _world.NewEntity();
            entity.Get<ChangeScoreRequest>() = new ChangeScoreRequest
            { 
                Type = type,
                Delta = delta
            };

            UnityEngine.Debug.Log($"[ScoreChangeApplier] Создан запрос изменения очков: " +
                $"Type={type} / " +
                $"Delta={delta}");
        }
    }
}
