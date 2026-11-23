using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    /// <summary>
    /// Makes sure the profile has the five queues. Does not touch ships.
    /// </summary>
    public sealed class EnsureSpaceQueuesStateAction : StateActionBase<SpaceProfile>
    {
        public override StateChangeSource Source => StateChangeSource.Space;

        public override StateActionValidationResult Validate(SpaceProfile state)
        {
            if (state == null)
                return StateActionValidationResult.Fail("Space profile is not loaded.", 901);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(SpaceProfile state)
        {
            if (state.Queues == null)
                state.Queues = new List<SpaceQueueState>();

            for (var i = 0; i < SpaceQueueIds.ALL.Length; i++)
            {
                var queueId = SpaceQueueIds.ALL[i];
                if (Find(state.Queues, queueId, out var queue))
                {
                    if (queue.MaxCount < 1)
                        queue.MaxCount = SpaceQueueIds.FREE_MAX_COUNT;
                    if (queue.Rows == null)
                        queue.Rows = new List<SpaceQueueRow>();
                    continue;
                }

                state.Queues.Add(new SpaceQueueState
                {
                    Id = queueId,
                    MaxCount = SpaceQueueIds.FREE_MAX_COUNT
                });
            }
        }

        private static bool Find(List<SpaceQueueState> queues, string queueId, out SpaceQueueState queue)
        {
            queue = null;
            for (var i = 0; i < queues.Count; i++)
            {
                var candidate = queues[i];
                if (candidate == null || candidate.Id != queueId)
                    continue;

                queue = candidate;
                return true;
            }

            return false;
        }
    }
}
