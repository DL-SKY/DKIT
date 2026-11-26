using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Circles the clicked hull. The ring stands off from its surface.
    /// </summary>
    public sealed class SpaceHoldOrbitCommand : SpaceCommand
    {
        public SpaceHoldOrbitCommand(string sourceId) : base(sourceId)
        {
        }

        public override void Execute(SpaceSessionHost session, string markerKey)
        {
            if (session == null)
                return;

            session.TrySetOrbit(markerKey);
        }
    }
}
