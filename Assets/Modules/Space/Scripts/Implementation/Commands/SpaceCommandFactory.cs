using Modules.Space.Scripts.Implementation.Definitions;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Builds the command for a pressed button. <paramref name="sourceId"/> is the hull or module def
    /// that granted it. Only a command whose constructor needs that id receives it.
    /// </summary>
    public static class SpaceCommandFactory
    {
        public static SpaceCommand Create(SpaceDefinitionsManager definitions, string commandId, string sourceId)
        {
            SpaceCommandDef command = null;
            if (definitions != null)
                definitions.TryGetCommand(commandId, out command);

            var commandType = command != null ? command.CommandType : SpaceCommandType.None;
            switch (commandType)
            {
                case SpaceCommandType.Approach:
                    return new SpaceApproachCommand(command);
                case SpaceCommandType.Stop:
                    return new SpaceStopCommand();
                default:
                    return new SpaceNoneCommand();
            }
        }
    }
}
