using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// One issued command. <see cref="SourceId"/> is the hull or module def that granted it.
    /// <see cref="Execute"/> writes the intent into the sector world.
    /// </summary>
    public abstract class SpaceCommand
    {
        protected SpaceCommand(string sourceId)
        {
            SourceId = sourceId ?? string.Empty;
        }

        public string SourceId { get; }

        public abstract void Execute(SpaceSessionHost session, string markerKey);
    }
}
