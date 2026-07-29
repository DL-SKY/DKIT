using UnityEngine;

namespace Modules.Utils.Scripts.Components
{
    /// <summary>
    /// Continuously rotates the GameObject. Drop onto an object — no code setup required.
    /// Useful for UI loading indicators (e.g. a spinning circular arrow).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Rotator : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Rotation speed in degrees per second. Negative value reverses direction.")]
        private float _speedDegreesPerSecond = 180f;

        [SerializeField]
        [Tooltip("Local axis to rotate around. For UI RectTransform use (0, 0, 1).")]
        private Vector3 _axis = Vector3.forward;

        [SerializeField]
        [Tooltip("If true, keeps spinning while the game is paused (Time.timeScale = 0).")]
        private bool _useUnscaledTime = true;

        [SerializeField]
        [Tooltip("If true, rotation starts automatically when the component is enabled.")]
        private bool _playOnEnable = true;


        private bool _isPlaying;


        public float SpeedDegreesPerSecond
        {
            get => _speedDegreesPerSecond;
            set => _speedDegreesPerSecond = value;
        }

        public Vector3 Axis
        {
            get => _axis;
            set => _axis = value;
        }

        public bool UseUnscaledTime
        {
            get => _useUnscaledTime;
            set => _useUnscaledTime = value;
        }

        public bool IsPlaying => _isPlaying;


        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.Rotate(_axis, _speedDegreesPerSecond * deltaTime, Space.Self);
        }


        public void Play()
        {
            _isPlaying = true;
        }

        public void Stop()
        {
            _isPlaying = false;
        }
    }
}
