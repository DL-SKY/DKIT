using Modules.Space.Scripts.Implementation.Definitions;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Orbits a hull from above or below. Drag rotates, scroll and pinch change distance.
    /// Distances and angles come from <see cref="SpaceCameraSettingsDef"/>.
    /// A click without a drag does not set a route.
    /// </summary>
    public sealed class SpaceOrbitCamera : MonoBehaviour
    {
        private SpaceCameraSettingsDef _settings;
        private Transform _target;
        private float _yaw;
        private float _pitch;
        private float _distance;
        private bool _pressed;
        private bool _dragging;
        private Vector2 _pressPosition;
        private Vector2 _previousPointer;
        private float _previousPinchDistance;

        public void Follow(Transform target, SpaceCameraSettingsDef settings)
        {
            _target = target;
            _settings = settings;
            if (_settings == null || !_settings.IsUsable())
            {
                UnityEngine.Debug.LogError("[Space] Orbit camera has no usable camera settings.");
                _settings = null;
                return;
            }

            _pitch = _settings.DefaultPitch;
            _distance = _settings.DefaultDistance;
            Apply();
        }

        private void LateUpdate()
        {
            if (_settings == null)
                return;

            ReadInput();
            Apply();
        }

        private void ReadInput()
        {
            if (UnityEngine.Input.touchCount >= 2)
            {
                _pressed = false;
                _dragging = false;
                ReadPinch();
                return;
            }

            _previousPinchDistance = 0f;

            if (UnityEngine.Input.touchCount == 1)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                ReadPointer(touch.phase == TouchPhase.Began, touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled, touch.position, touch.deltaPosition);
                return;
            }

            var mousePosition = (Vector2)UnityEngine.Input.mousePosition;
            if (UnityEngine.Input.GetMouseButtonDown(0))
                _previousPointer = mousePosition;

            var mouseDelta = mousePosition - _previousPointer;
            _previousPointer = mousePosition;
            ReadPointer(
                UnityEngine.Input.GetMouseButtonDown(0),
                UnityEngine.Input.GetMouseButtonUp(0),
                mousePosition,
                mouseDelta);
            _distance = Mathf.Clamp(_distance - UnityEngine.Input.mouseScrollDelta.y * _settings.ScrollStep, _settings.MinDistance, _settings.MaxDistance);
        }

        private void ReadPointer(bool began, bool ended, Vector2 position, Vector2 delta)
        {
            if (began)
            {
                _pressed = true;
                _dragging = false;
                _pressPosition = position;
                return;
            }

            if (!_pressed)
                return;

            var threshold = _settings.DragThresholdPixels;
            if ((position - _pressPosition).sqrMagnitude >= threshold * threshold)
                _dragging = true;

            if (_dragging)
            {
                _yaw += delta.x * _settings.DegreesPerPixel;
                _pitch = Mathf.Clamp(_pitch - delta.y * _settings.DegreesPerPixel, _settings.MinPitch, _settings.MaxPitch);
            }

            if (ended)
            {
                _pressed = false;
                _dragging = false;
            }
        }

        private void ReadPinch()
        {
            var first = UnityEngine.Input.GetTouch(0).position;
            var second = UnityEngine.Input.GetTouch(1).position;
            var pinchDistance = Vector2.Distance(first, second);
            if (_previousPinchDistance > 0f && pinchDistance > 0f)
                _distance = Mathf.Clamp(_distance * (_previousPinchDistance / pinchDistance), _settings.MinDistance, _settings.MaxDistance);

            _previousPinchDistance = pinchDistance;
        }

        private void Apply()
        {
            if (_target == null)
                return;

            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _target.position + rotation * new Vector3(0f, 0f, -_distance);
            transform.LookAt(_target.position);
        }
    }
}
