using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Drops the route and stays. The hull brakes by engine inertia and does not fly to a tapped point.
    /// </summary>
    public sealed class SpaceHoldPositionCommand : SpaceCommand
    {
        public SpaceHoldPositionCommand(string sourceId) : base(sourceId)
        {
        }

        public override void Execute(SpaceSessionHost session, string markerKey)
        {
            if (session == null)
                return;

            session.TryHoldPosition();
        }
    }
}
