using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// One issued command. <see cref="Execute"/> writes the intent into the sector world.
    /// </summary>
    public abstract class SpaceCommand
    {
        public abstract void Execute(SpaceSessionHost session, string markerKey);
    }
}
