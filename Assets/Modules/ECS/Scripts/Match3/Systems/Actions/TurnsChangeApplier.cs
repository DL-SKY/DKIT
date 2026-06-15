using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.ECS.Scripts.Match3.Components;

namespace Modules.ECS.Scripts.Match3.Systems.Actions
{
    public class TurnsChangeApplier : IActionApplier
    {
        private EcsWorld _world;

        public IActionApplier Init(EcsWorld world)
        {
            _world = world;
            return this;
        }

        public void Apply(MatchAction action)
        {
            if (action.Type != MatchActionType.TurnsChange)
                return;

            //ChangeTurnsRequest
            var delta = action.IntParameter1;
            var entity = _world.NewEntity();
            entity.Get<ChangeTurnsRequest>() = new ChangeTurnsRequest
            {
                Delta = delta
            };

            UnityEngine.Debug.Log($"[TurnsChangeApplier] Создан запрос изменения счетчика ходов: " +
                $"Delta={delta}");
        }
    }
}
