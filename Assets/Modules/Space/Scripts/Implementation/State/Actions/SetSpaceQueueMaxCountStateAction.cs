using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    /// <summary>
    /// Changes how many rows one queue may hold.
    /// </summary>
    public sealed class SetSpaceQueueMaxCountStateAction : StateActionBase<SpaceProfile>
    {
        private readonly string _queueId;
        private readonly int _maxCount;

        public SetSpaceQueueMaxCountStateAction(string queueId, int maxCount)
        {
            _queueId = queueId;
            _maxCount = maxCount;
        }

        public override StateChangeSource Source => StateChangeSource.Space;

        public override StateActionValidationResult Validate(SpaceProfile state)
        {
            if (state == null)
                return StateActionValidationResult.Fail("Space profile is not loaded.", 921);

            if (_maxCount < 1)
                return StateActionValidationResult.Fail("Queue max count must be at least 1.", 922);

            if (!SpaceProfileQueues.TryGet(state, _queueId, out _))
                return StateActionValidationResult.Fail("Queue was not found.", 923);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(SpaceProfile state)
        {
            SpaceProfileQueues.TryGet(state, _queueId, out var queue);
            queue.MaxCount = _maxCount;
        }
    }
}
