using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using UnityEngine;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Hints
{
    public sealed class HintViewModel : ViewModelBase
    {
        [Inject] private readonly WindowsManager _windowsManager;

        private bool _isDisposed;
        private bool _closedRaised;

        public string LocalizationKey { get; private set; } = string.Empty;
        public float DurationSeconds { get; private set; }

        /// <summary>
        /// Raised once when the hint view is destroyed (after hide animation).
        /// </summary>
        public event Action Closed;

        public void Init(string localizationKey, float durationSeconds)
        {
            _isDisposed = false;
            _closedRaised = false;
            LocalizationKey = localizationKey ?? string.Empty;
            DurationSeconds = Mathf.Max(1.0f, durationSeconds);
        }

        public void RequestClose()
        {
            if (_isDisposed)
                return;

            if (ViewHandle == 0)
                return;

            _windowsManager.CloseView(ViewHandle);
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            LocalizationKey = string.Empty;

            // Only notify completion for hints that were actually opened as a view.
            if (!_closedRaised && ViewHandle != 0)
            {
                _closedRaised = true;
                Closed?.Invoke();
            }

            Closed = null;
        }

        protected override Options CreateOptions()
        {
            return new Options(
                canCloseOnEsc: false,
                hideInHistory: true,
                sortingLayer: SortingOrderLayer.HINT);
        }
    }
}
