#if UNITY_EDITOR
using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Scene-view sphere for a body radius. Radius is meters from the center, not a diameter.
    /// Present only in the editor. Nothing in play reads it.
    /// </summary>
    public sealed class SpaceRadiusGizmo : MonoBehaviour
    {
        public static readonly Color HULL_COLOR = new Color(0.3f, 0.75f, 1f, 0.95f);

        public float Radius;
        public Color Color = HULL_COLOR;

        public static Color ColorFor(SpaceObjectType objectType)
        {
            switch (objectType)
            {
                case SpaceObjectType.Asteroid:
                    return new Color(1f, 0.85f, 0.2f, 0.95f);
                case SpaceObjectType.Station:
                    return new Color(0.2f, 0.9f, 0.85f, 0.95f);
                case SpaceObjectType.PointOfInterest:
                    return new Color(0.45f, 1f, 0.5f, 0.95f);
                default:
                    return new Color(1f, 1f, 1f, 0.95f);
            }
        }

        private void OnDrawGizmos()
        {
            if (Radius <= 0f)
                return;

            Gizmos.color = Color;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
#endif
