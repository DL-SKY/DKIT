using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using UnityEngine;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Hints
{
    public enum HintViewStateType
    {
        None,
        Show,
        Hold,
        Hide
    }

    public sealed class HintViewModel : ViewModelBase
    {
        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly Updater _updater;

        /// <summary>
        /// Raised once after the hint view is destroyed. Used by <see cref="HintManager"/>.
        /// </summary>
        public event Action Closed;

        /// <summary>
        /// Raised once when hide animation finishes. View should destroy itself.
        /// </summary>
        public event Action HideFinished;

        public string LocalizationKey { get; private set; } = string.Empty;
        public float Alpha { get; private set; }

        private const float MAX_DELTA_TIME = 1f / 30f;

        private HintViewStateType _state;
        private float _lifeTime;
        private float _animationElapsed;
        private float _animationDuration;
        private float _alphaFrom;
        private float _alphaTo;
        private bool _isDisposed;
        private bool _closedRaised;
        private bool _hideFinishedRaised;

        protected override Options CreateOptions()
        {
            return new Options(
                canCloseOnEsc: false,
                hideInHistory: true,
                sortingLayer: SortingOrderLayer.HINT);
        }

        public void Init(string localizationKey, float durationSeconds)
        {
            LocalizationKey = localizationKey ?? string.Empty;
            Alpha = 0.0f;

            _state = HintViewStateType.None;
            _lifeTime = Mathf.Max(1.0f, durationSeconds);
            _animationElapsed = 0.0f;
            _animationDuration = 0.0f;
            _alphaFrom = 0.0f;
            _alphaTo = 0.0f;
            _isDisposed = false;
            _closedRaised = false;
            _hideFinishedRaised = false;

            _updater.OnUpdate += OnUpdateHandler;
        }

        public void Show(float duration)
        {
            if (_isDisposed || _state == HintViewStateType.Hide)
                return;

            _state = HintViewStateType.Show;
            StartFade(1.0f, duration);
        }

        public void Hide(float duration)
        {
            if (_isDisposed || _state == HintViewStateType.Hide)
                return;

            _state = HintViewStateType.Hide;
            StartFade(0.0f, duration);
        }

        public void RequestClose()
        {
            if (_isDisposed || ViewHandle == 0)
                return;

            _windowsManager.CloseView(ViewHandle);
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            LocalizationKey = string.Empty;
            _state = HintViewStateType.None;

            _updater.OnUpdate -= OnUpdateHandler;

            SendClosed();
            HideFinished = null;
        }

        private void OnUpdateHandler(float deltaTime)
        {
            if (_isDisposed || _state == HintViewStateType.None)
                return;

            float dt = Mathf.Min(deltaTime, MAX_DELTA_TIME);

            if (_state == HintViewStateType.Show || _state == HintViewStateType.Hide)
                TickFade(dt);

            if (_state == HintViewStateType.Hold)
            {
                _lifeTime -= dt;
                if (_lifeTime <= 0.0f)
                    RequestClose();
            }
        }

        private void StartFade(float targetAlpha, float duration)
        {
            _alphaFrom = Alpha;
            _alphaTo = targetAlpha;
            _animationElapsed = 0.0f;
            _animationDuration = Mathf.Max(0.0f, duration);

            if (_animationDuration <= 0.0f)
            {
                Alpha = _alphaTo;
                SendOnChange();
                CompleteFade();
            }
        }

        private void TickFade(float deltaTime)
        {
            _animationElapsed += deltaTime;
            float t = Mathf.Clamp01(_animationElapsed / _animationDuration);
            Alpha = Mathf.Lerp(_alphaFrom, _alphaTo, t);
            SendOnChange();

            if (t >= 1.0f)
                CompleteFade();
        }

        private void CompleteFade()
        {
            if (_state == HintViewStateType.Show)
            {
                _state = HintViewStateType.Hold;
                return;
            }

            if (_state == HintViewStateType.Hide)
            {
                _state = HintViewStateType.None;
                SendHideFinished();
            }
        }

        private void SendHideFinished()
        {
            if (_hideFinishedRaised)
                return;

            _hideFinishedRaised = true;
            HideFinished?.Invoke();
        }

        private void SendClosed()
        {
            if (_closedRaised || ViewHandle == 0)
                return;

            _closedRaised = true;
            Closed?.Invoke();
            Closed = null;
        }
    }
}
