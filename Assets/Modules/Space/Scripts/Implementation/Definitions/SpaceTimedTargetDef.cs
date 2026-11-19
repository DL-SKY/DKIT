using Modules.Definitions.Scripts.Defs;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Temporary stand-in for a queued target. This type and the TimedTargets folder will be removed.
    /// A queue row will store the id of a skill, blueprint, or other target definition, and that definition will carry BaseTimeMs.
    /// </summary>
    public sealed class SpaceTimedTargetDef : AbstractDefinition
    {
        public SpaceQueueKind Kind;
        public long BaseTimeMs;
    }
}
