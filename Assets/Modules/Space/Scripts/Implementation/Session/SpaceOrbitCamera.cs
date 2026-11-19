using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Orbits behind and above a hull. Drag rotates, scroll and pinch change distance.
    /// A click without a drag does not set a route.
    /// </summary>
    public sealed class SpaceOrbitCamera : MonoBehaviour
    {
        private const float DEFAULT_DISTANCE = 14f;
        private const float MIN_DISTANCE = 4f;
        private const float MAX_DISTANCE = 48f;
        private const float DEFAULT_PITCH = 22f;
        private const float MIN_PITCH = 10f;
        private const float MAX_PITCH = 75f;
        private const float DEGREES_PER_PIXEL = 0.2f;
        private const float SCROLL_STEP = 2f;
        private const float DRAG_THRESHOLD_PIXELS = 6f;

        private Transform _target;
        private float _yaw;
        private float _pitch = DEFAULT_PITCH;
        private float _distance = DEFAULT_DISTANCE;
        private bool _pressed;
        private bool _dragging;
        private Vector2 _pressPosition;
        private Vector2 _previousPointer;
        private float _previousPinchDistance;

        public void Follow(Transform target)
        {
            _target = target;
            Apply();
        }

        private void LateUpdate()
        {
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
            _distance = Mathf.Clamp(_distance - UnityEngine.Input.mouseScrollDelta.y * SCROLL_STEP, MIN_DISTANCE, MAX_DISTANCE);
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

            if ((position - _pressPosition).sqrMagnitude >= DRAG_THRESHOLD_PIXELS * DRAG_THRESHOLD_PIXELS)
                _dragging = true;

            if (_dragging)
            {
                _yaw += delta.x * DEGREES_PER_PIXEL;
                _pitch = Mathf.Clamp(_pitch - delta.y * DEGREES_PER_PIXEL, MIN_PITCH, MAX_PITCH);
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
                _distance = Mathf.Clamp(_distance * (_previousPinchDistance / pinchDistance), MIN_DISTANCE, MAX_DISTANCE);

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
