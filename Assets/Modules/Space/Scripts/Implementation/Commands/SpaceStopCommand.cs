using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Clears the route point. Speed then falls to zero by the engine inertia. Other orders stay.
    /// </summary>
    public sealed class SpaceStopCommand : SpaceCommand
    {
        public SpaceStopCommand(string sourceId) : base(sourceId)
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
