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
    /// Kind copied from the object definition. The player hull does not use this.
    /// </summary>
    public struct SpaceKind
    {
        public SpaceObjectKind Value;
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
    /// World point the body is moving toward. The movement system consumes it.
    /// </summary>
    public struct SpaceMoveTarget
    {
        public Vector3 Position;
    }
}
