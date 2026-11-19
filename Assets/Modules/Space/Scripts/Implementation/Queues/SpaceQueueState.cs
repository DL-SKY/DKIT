using System.Collections.Generic;

namespace Modules.Space.Scripts.Implementation.Queues
{
    public sealed class SpaceQueueState
    {
        public string Id;
        public int MaxCount = SpaceQueueIds.FREE_MAX_COUNT;
        public List<SpaceQueueRow> Rows = new List<SpaceQueueRow>();
    }
}
