using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    internal static class SpaceMarkerScreen
    {
        public static bool TryLocalPoint(RectTransform holder, Camera camera, Vector3 world, float edgeMargin, out Vector2 local)
        {
            local = Vector2.zero;
            if (holder == null || camera == null)
                return false;

            var screen = Project(camera, world, edgeMargin);
            var canvas = holder.GetComponentInParent<Canvas>();
            var root = canvas != null ? canvas.rootCanvas : null;
            var eventCamera = root != null && root.renderMode != RenderMode.ScreenSpaceOverlay ? root.worldCamera : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(holder, screen, eventCamera, out local);
        }

        private static Vector2 Project(Camera camera, Vector3 world, float edgeMargin)
        {
            var screen = camera.WorldToScreenPoint(world);
            var center = new Vector2(Screen.width, Screen.height) * 0.5f;
            var point = new Vector2(screen.x, screen.y);
            if (screen.z < 0f)
                point = center - (point - center);

            var max = new Vector2(Screen.width - edgeMargin, Screen.height - edgeMargin);
            var inside = screen.z >= 0f && point.x >= edgeMargin && point.x <= max.x && point.y >= edgeMargin && point.y <= max.y;
            if (inside)
                return point;

            var fromCenter = point - center;
            if (fromCenter.sqrMagnitude < 0.01f)
                fromCenter = Vector2.up;

            var half = max - center;
            var scaleX = half.x / Mathf.Abs(fromCenter.x);
            var scaleY = half.y / Mathf.Abs(fromCenter.y);
            return center + fromCenter * Mathf.Min(scaleX, scaleY);
        }
    }
}
