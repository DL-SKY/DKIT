using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    public struct SpaceMarkerSnapshot
    {
        public string TargetId;
        public string DefId;
        public bool Player;
        public SpaceObjectKind Kind;
        public Vector3 WorldPosition;
    }
}
