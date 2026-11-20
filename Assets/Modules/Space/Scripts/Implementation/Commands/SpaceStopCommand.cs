using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Drops the move target so the player hull brakes on its current heading.
    /// </summary>
    public sealed class SpaceStopCommand : SpaceCommand
    {
        public SpaceStopCommand()
        {
        }

        public override void Execute(SpaceSessionHost session, string markerKey)
        {
            if (session == null)
                return;

            session.TryStopPlayer();
        }
    }
}
