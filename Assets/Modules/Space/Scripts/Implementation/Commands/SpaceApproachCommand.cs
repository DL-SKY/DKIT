using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Flies the player hull toward the clicked marker.
    /// </summary>
    public sealed class SpaceApproachCommand : SpaceCommand
    {
        private readonly SpaceCommandDef _command;

        public SpaceApproachCommand(SpaceCommandDef command, string sourceId) : base(sourceId)
        {
            _command = command;
        }

        public override void Execute(SpaceSessionHost session, string markerKey)
        {
            if (_command == null || session == null)
                return;

            session.TrySetMoveTarget(markerKey);
        }
    }
}
