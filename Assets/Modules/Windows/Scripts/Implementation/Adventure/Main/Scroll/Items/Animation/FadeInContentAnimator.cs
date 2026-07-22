using System;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation
{
    /// <summary>
    /// Fades content in via <see cref="CanvasGroup"/> alpha from 0 to 1 over a configured duration.
    /// Suitable for Image items and any other content wrapped by a CanvasGroup.
    /// </summary>
    public sealed class FadeInContentAnimator : MonoBehaviour, IContentAnimator
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _duration = 0.5f;

        private bool _isPlaying;
        private bool _isCompleted;
        private float _elapsed;

        public bool IsPlaying => _isPlaying;

        public event Action Completed;

        private void Awake()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }

        public void Play()
        {
            if (_isCompleted || _isPlaying)
                return;

            if (_canvasGroup == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(FadeInContentAnimator)}] CanvasGroup is not assigned on '{name}'. Completing immediately.",
                    this);
                Complete();
                return;
            }

            _canvasGroup.alpha = 0f;
            _elapsed = 0f;

            if (_duration <= 0f)
            {
                Complete();
                return;
            }

            _isPlaying = true;
        }

        public void Skip()
        {
            if (_isCompleted)
                return;

            Complete();
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            _canvasGroup.alpha = t;

            if (t >= 1f)
                Complete();
        }

        private void Complete()
        {
            _isPlaying = false;
            _isCompleted = true;

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;

            Completed?.Invoke();
        }
    }
}
