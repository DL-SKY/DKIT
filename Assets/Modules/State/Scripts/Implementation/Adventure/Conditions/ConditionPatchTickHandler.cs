using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Conditions;

namespace Modules.State.Scripts.Implementation.Adventure.Conditions
{
    public class ConditionPatchTickHandler : IConditionTickHandler
    {
        public bool CanHandle(ConditionDef conditionDef)
        {
            return conditionDef?.Tick?.TickPatch != null;
        }

        public void ApplyTick(ConditionTickContext context, ConditionDef conditionDef)
        {
            if (context?.Character == null || context.DefinitionsManager == null)
                return;

            CharacterParametersOperator.ApplyPatch(
                context.Character,
                conditionDef.Tick.TickPatch,
                context.DefinitionsManager);
        }
    }
}
