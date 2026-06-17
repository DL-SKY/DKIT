using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Interfaces;

namespace Modules.State.Scripts.Actions.Interfaces
{
    public interface IStateLogic<TStateData> where TStateData : class, IStateData, new()
    {
        StateActionValidationResult ProcessAction(IStateAction<TStateData> action, bool forceBatch = false);
    }
}
