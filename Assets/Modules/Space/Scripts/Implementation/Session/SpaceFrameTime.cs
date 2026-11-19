namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Frame delta written by the session controller immediately before each ECS tick.
    /// </summary>
    public sealed class SpaceFrameTime
    {
        public float DeltaSeconds { get; set; }
    }
}
