using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// A command that does not touch the sector world.
    /// </summary>
    public sealed class SpaceNoneCommand : SpaceCommand
    {
        public SpaceNoneCommand()
        {
        }

        public override void Execute(SpaceSessionHost session, string markerKey)
        {
        }
    }
}
