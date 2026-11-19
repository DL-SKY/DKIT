namespace Modules.ECS.Scripts.Space.Components
{
    /// <summary>
    /// Marker of the single session entity. No ship, contact, or order data.
    /// </summary>
    public struct SpaceSession
    {
    }

    /// <summary>
    /// Simulation clock on the session entity.
    /// Seconds come from the frame delta, not from the queue time formula.
    /// </summary>
    public struct SpaceClock
    {
        public float DeltaSeconds;
        public double ElapsedSeconds;
    }
}
