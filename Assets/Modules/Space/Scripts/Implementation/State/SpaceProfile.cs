using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.State
{
    public sealed class SpaceProfile
    {
        public List<SpaceQueueState> Queues = new List<SpaceQueueState>();
    }
}
