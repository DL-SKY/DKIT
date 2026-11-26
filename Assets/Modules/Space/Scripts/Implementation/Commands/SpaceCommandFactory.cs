using Modules.Space.Scripts.Implementation.Definitions;

namespace Modules.Space.Scripts.Implementation.Commands
{
    /// <summary>
    /// Builds the command for a pressed button. <paramref name="sourceId"/> is the hull or module def
    /// that granted it. Every command receives that id through <see cref="SpaceCommand"/>.
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
                    return new SpaceApproachCommand(command, sourceId);
                case SpaceCommandType.Stop:
                    return new SpaceStopCommand(sourceId);
                case SpaceCommandType.HoldPosition:
                    return new SpaceHoldPositionCommand(sourceId);
                case SpaceCommandType.HoldOrbit:
                    return new SpaceHoldOrbitCommand(sourceId);
                default:
                    return new SpaceNoneCommand(sourceId);
            }
        }
    }
}
