using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Conditions;

namespace Modules.State.Scripts.Implementation.Adventure.Conditions
{
    public interface IConditionTickHandler
    {
        bool CanHandle(ConditionDef conditionDef);
        void ApplyTick(ConditionTickContext context, ConditionDef conditionDef);
    }
}
