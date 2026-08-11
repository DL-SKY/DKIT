using Modules.Windows.Scripts.Implementation.Hints;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Managers
{
    /// <summary>
    /// Default <see cref="IHintManager"/>: queues hint view-models and shows one at a time via <see cref="WindowsManager"/>.
    /// Place on the WindowsManager prefab (or any GO that shares the same DI-instantiated hierarchy).
    /// </summary>
    public sealed class HintManager : MonoBehaviour, IHintManager
    {
        [SerializeField] private float _defaultDurationSeconds = 3f;

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        private readonly Queue<HintViewModel> _queue = new Queue<HintViewModel>();
        private HintViewModel _active;
        private bool _suppressAdvanceOnClose;

        public void Show(string localizationKey)
        {
            Show(localizationKey, _defaultDurationSeconds);
        }

        public void Show(string localizationKey, float durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(localizationKey))
            {
                UnityEngine.Debug.LogWarning($"[{nameof(HintManager)}] Localization key is empty.");
                return;
            }

            if (_viewModelFactory == null || _windowsManager == null)
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(HintManager)}] Dependencies are not injected. Is the component on the WindowsManager prefab?");
                return;
            }

            HintViewModel viewModel = _viewModelFactory.Create<HintViewModel>();
            viewModel.Init(localizationKey, durationSeconds);
            _queue.Enqueue(viewModel);
            TryPresentNext();
        }

        public void Clear()
        {
            while (_queue.Count > 0)
            {
                HintViewModel pending = _queue.Dequeue();
                pending.Closed -= OnActiveClosed;
                pending.Dispose();
            }

            if (_active == null)
                return;

            _suppressAdvanceOnClose = true;
            _active.RequestClose();
        }

        private void TryPresentNext()
        {
            if (_active != null)
                return;

            if (_queue.Count == 0)
                return;

            _active = _queue.Dequeue();
            _active.Closed += OnActiveClosed;
            _windowsManager.OpenView<HintView, HintViewModel>(HintView.Path, _active);
        }

        private void OnActiveClosed()
        {
            if (_active != null)
                _active.Closed -= OnActiveClosed;

            _active = null;

            bool suppress = _suppressAdvanceOnClose;
            _suppressAdvanceOnClose = false;

            if (!suppress)
                TryPresentNext();
        }

        private void OnDestroy()
        {
            _suppressAdvanceOnClose = true;

            while (_queue.Count > 0)
            {
                HintViewModel pending = _queue.Dequeue();
                pending.Closed -= OnActiveClosed;
                pending.Dispose();
            }

            if (_active != null)
            {
                _active.Closed -= OnActiveClosed;
                _active = null;
            }
        }
    }
}
