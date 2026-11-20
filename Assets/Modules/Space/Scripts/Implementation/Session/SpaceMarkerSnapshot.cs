using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// One marker target. <see cref="Ship"/> means <see cref="DefId"/> is a hull id.
    /// The picture then comes from that hull. Otherwise <see cref="ObjectType"/> selects the picture
    /// from the single marker-icon def. <see cref="Relation"/> is how this marker stands toward the player.
    /// </summary>
    public struct SpaceMarkerSnapshot
    {
        public string TargetId;
        public string DefId;
        public bool Ship;
        public bool Player;
        public SpaceObjectType ObjectType;
        public SpaceRelation Relation;
        public Vector3 WorldPosition;
        public float MetersPerSecond;
        public float MaxMetersPerSecond;
    }
}
