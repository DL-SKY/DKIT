using Modules.Definitions.Scripts.Defs;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Shared record for a skill or any other queued target. Base time stays on the definition.
    /// </summary>
    public sealed class SpaceTimedTargetDef : AbstractDefinition
    {
        public SpaceQueueKind Kind;
        public long BaseTimeMs;
    }
}
