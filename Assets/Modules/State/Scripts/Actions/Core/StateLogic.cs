using Modules.State.Scripts.Actions.Interfaces;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Interfaces;
using System;

namespace Modules.State.Scripts.Actions.Core
{
    public class StateLogic<TStateData> : IStateLogic<TStateData>
        where TStateData : class, IStateData, new()
    {
        private readonly IStateManager<TStateData> _stateManager;
        private readonly Action _saveAction;
        private readonly int _batchSize;

        private int _pendingActionCount;

        public StateLogic(IStateManager<TStateData> stateManager, Action saveAction, int batchSize)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _saveAction = saveAction;

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");

            _batchSize = batchSize;
        }

        public StateActionValidationResult ProcessAction(IStateAction<TStateData> action, bool forceBatch = false)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var state = _stateManager.State;
            if (state == null)
                return StateActionValidationResult.Fail("State is not loaded.", 1);

            var validationResult = action.Validate(state);
            if (validationResult == null)
                return StateActionValidationResult.Fail("Action returned null validation result.", 2);

            if (!validationResult.IsValid)
                return validationResult;

            action.Execute(state);

            _pendingActionCount++;

            if (forceBatch || _pendingActionCount >= _batchSize)
                _saveAction?.Invoke();

            if (_pendingActionCount >= _batchSize)
                _pendingActionCount = 0;

            return StateActionValidationResult.Ok;
        }
    }
}
