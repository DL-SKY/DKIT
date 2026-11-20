namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// How a marker stands toward the player. Another hull is <see cref="Neutral"/> until factions exist.
    /// </summary>
    public enum SpaceRelation
    {
        Player,
        Ally,
        Enemy,
        Neutral
    }
}
