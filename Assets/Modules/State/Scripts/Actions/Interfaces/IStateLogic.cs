using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Interfaces;
using System;

namespace Modules.State.Scripts.Actions.Interfaces
{
    public interface IStateLogic<TStateData> where TStateData : class, IStateData, new()
    {
        event Action<StateChangeSource> StateChanged;

        StateActionValidationResult ProcessAction(IStateAction<TStateData> action, bool forceBatch = false);
    }
}
