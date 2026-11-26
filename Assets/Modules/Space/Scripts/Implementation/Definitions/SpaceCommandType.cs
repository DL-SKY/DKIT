namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// What a <see cref="SpaceCommandDef"/> does when its button is pressed.
    /// <see cref="None"/> changes nothing in the sector world.
    /// </summary>
    public enum SpaceCommandType
    {
        None,
        Approach,
        Stop,
        HoldPosition,
        HoldOrbit
    }
}
