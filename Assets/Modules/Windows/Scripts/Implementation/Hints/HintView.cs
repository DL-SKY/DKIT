using Modules.Localization.Scripts.Components;
using Modules.Windows.Scripts.Base;
using System.Collections;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Hints
{
    /// <summary>
    /// Toast hint: fade in, hold for <see cref="HintViewModel.DurationSeconds"/>, fade out, destroy.
    /// Prefab should size height from text via layout + ContentSizeFitter.
    /// </summary>
    public sealed class HintView : ViewBase<HintViewModel>
    {
        public static string Path = "Prefabs/Views/Hints/HintView";

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LocalizationText _text;
        [SerializeField] private float _showAnimationSeconds = 0.25f;
        [SerializeField] private float _hideAnimationSeconds = 0.25f;

        private bool _isClosing;
        private Coroutine _lifecycleRoutine;
        private Coroutine _closeRoutine;

        protected override void InitImplementation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_text != null)
            {
                _text.SetText(_viewModel.LocalizationKey);
            }
        }

        protected override void Subscribe()
        {
        }

        protected override void Unsubscribe()
        {
        }

        public override void Show()
        {
            if (_lifecycleRoutine != null)
                StopCoroutine(_lifecycleRoutine);

            _lifecycleRoutine = StartCoroutine(LifecycleRoutine());
        }

        public override void Hide()
        {
            if (_isClosing)
                return;

            _isClosing = true;

            if (_lifecycleRoutine != null)
            {
                StopCoroutine(_lifecycleRoutine);
                _lifecycleRoutine = null;
            }

            if (_closeRoutine != null)
            {
                StopCoroutine(_closeRoutine);
            }

            _closeRoutine = StartCoroutine(CloseRoutine());
        }

        private IEnumerator LifecycleRoutine()
        {
            yield return FadeRoutine(0f, 1f, _showAnimationSeconds);

            float hold = _viewModel != null ? _viewModel.DurationSeconds : 0f;
            float elapsed = 0f;
            while (elapsed < hold && !_isClosing)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _lifecycleRoutine = null;

            if (!_isClosing)
                Hide();
        }

        private IEnumerator CloseRoutine()
        {
            float from = _canvasGroup != null ? _canvasGroup.alpha : 1f;
            yield return FadeRoutine(from, 0f, _hideAnimationSeconds);
            _closeRoutine = null;
            Destroy(gameObject);
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            if (_canvasGroup == null || duration <= 0f)
            {
                if (_canvasGroup != null)
                    _canvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            _canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _canvasGroup.alpha = to;
        }
    }
}
