using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Actions.Interfaces
{
    public interface IStateAction<TStateData> where TStateData : class, IStateData, new()
    {
        StateChangeSource Source { get; }

        StateActionValidationResult Validate(TStateData state);
        void Execute(TStateData state);
    }
}
