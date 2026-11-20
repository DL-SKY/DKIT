using Leopotam.Ecs;
using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.ECS.Scripts.Space.Components
{
    /// <summary>
    /// World position in sector meters. Movement writes it. The view copies it out.
    /// </summary>
    public struct SpacePosition
    {
        public Vector3 Value;
    }

    /// <summary>
    /// Yaw in degrees around the vertical axis. Zero looks along +Z.
    /// Rotation writes it. The view copies it out. The camera does not read it.
    /// </summary>
    public struct SpaceYaw
    {
        public float Degrees;
    }

    /// <summary>
    /// Sphere radius in meters around the position. Zero does not push other bodies.
    /// </summary>
    public struct SpaceRadius
    {
        public float Meters;
    }

    /// <summary>
    /// Object type copied from the object definition. Hulls do not use this.
    /// Their marker type is <see cref="SpaceObjectType.Ship"/>.
    /// </summary>
    public struct SpaceBodyType
    {
        public SpaceObjectType Value;
    }

    /// <summary>
    /// Visual link. The transform is not the source of the pose.
    /// </summary>
    public struct SpaceView
    {
        public Transform Transform;
    }

    /// <summary>
    /// Marker of the single player hull entity.
    /// </summary>
    public struct SpacePlayerHull
    {
    }

    /// <summary>
    /// Marker of any hull, including the player's. A hull without <see cref="SpacePlayerHull"/> is another ship.
    /// </summary>
    public struct SpaceHull
    {
    }

    /// <summary>
    /// Short name drawn on the screen marker. The marker reads it and does not invent a title.
    /// </summary>
    public struct SpaceLabel
    {
        public string Value;
    }

    /// <summary>
    /// Stable marker key assigned once at spawn. The window matches a button to this value.
    /// </summary>
    public struct SpaceMarkerKey
    {
        public string Value;
    }

    /// <summary>
    /// Turn rate copied from the hull maneuverability. Present does not start a turn by itself.
    /// </summary>
    public struct SpaceTurnRate
    {
        public float DegreesPerSecond;
    }

    /// <summary>
    /// Engine motion on the hull entity. Maximum speed and inertia come from the fitted engine.
    /// MetersPerSecond is the current speed along the nose.
    /// </summary>
    public struct SpaceMotion
    {
        public float MaxMetersPerSecond;
        public float InertiaSeconds;
        public float MetersPerSecond;
    }

    /// <summary>
    /// Desired yaw written by an order. The rotation system consumes it.
    /// </summary>
    public struct SpaceDesiredYaw
    {
        public float Degrees;
    }

    /// <summary>
    /// Entity the body is moving toward. Face and movement read its live position and radius.
    /// </summary>
    public struct SpaceMoveTarget
    {
        public EcsEntity Target;
    }
}
