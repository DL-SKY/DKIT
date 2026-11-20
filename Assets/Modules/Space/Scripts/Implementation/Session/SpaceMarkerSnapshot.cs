using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// One marker target. <see cref="Ship"/> means <see cref="DefId"/> is a hull id.
    /// The picture then comes from that hull. Otherwise <see cref="Kind"/> selects the picture
    /// from the single marker-icon def. <see cref="Player"/> is the player's own ship.
    /// </summary>
    public struct SpaceMarkerSnapshot
    {
        public string TargetId;
        public string DefId;
        public bool Ship;
        public bool Player;
        public SpaceObjectKind Kind;
        public Vector3 WorldPosition;
        public float MetersPerSecond;
        public float MaxMetersPerSecond;
    }
}
