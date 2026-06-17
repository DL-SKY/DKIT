using Modules.State.Scripts.Actions.Interfaces;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Actions.Core
{
    public abstract class StateActionBase<TStateData> : IStateAction<TStateData>
        where TStateData : class, IStateData, new()
    {
        public virtual StateActionValidationResult Validate(TStateData state)
        {
            return StateActionValidationResult.Ok;
        }

        public abstract void Execute(TStateData state);
    }
}
