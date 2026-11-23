using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    internal static class SpaceProfileQueues
    {
        public static bool TryGet(SpaceProfile profile, string queueId, out SpaceQueueState queue)
        {
            queue = null;
            var queues = profile?.Queues;
            if (queues == null || string.IsNullOrEmpty(queueId))
                return false;

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
