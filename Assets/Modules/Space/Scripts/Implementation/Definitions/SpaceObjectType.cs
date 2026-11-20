namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// What a marker is. <see cref="Ship"/> is any hull. The other values are placed bodies.
    /// Relation to the player is <see cref="SpaceRelation"/>, not a value here.
    /// </summary>
    public enum SpaceObjectType
    {
        PointOfInterest,
        Station,
        Asteroid,
        Spawn,
        Ship
    }
}
