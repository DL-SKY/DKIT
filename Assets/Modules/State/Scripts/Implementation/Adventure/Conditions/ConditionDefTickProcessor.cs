using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Conditions;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Conditions
{
    public static class ConditionDefTickProcessor
    {
        private static readonly List<IConditionTickHandler> HANDLERS = new List<IConditionTickHandler>
        {
            new ConditionPatchTickHandler(),
            new ConditionDamageTickHandler(),
        };

        public static void ApplyTurnTick(
            CharacterStateData character,
            string conditionFeatId,
            DefinitionsManager definitionsManager)
        {
            if (character == null
                || definitionsManager?.Conditions == null
                || string.IsNullOrWhiteSpace(conditionFeatId))
            {
                return;
            }

            if (!definitionsManager.Conditions.TryGetValue(conditionFeatId, out ConditionDef conditionDef)
                || conditionDef == null
                || conditionDef.Disabled
                || conditionDef.Tick == null
                || conditionDef.Tick.Disabled)
            {
                return;
            }

            var context = new ConditionTickContext
            {
                ConditionFeatId = conditionFeatId,
                Character = character,
                DefinitionsManager = definitionsManager,
            };

            for (int i = 0; i < HANDLERS.Count; i++)
            {
                IConditionTickHandler handler = HANDLERS[i];
                if (!handler.CanHandle(conditionDef))
                    continue;

                handler.ApplyTick(context, conditionDef);
            }
        }
    }
}
