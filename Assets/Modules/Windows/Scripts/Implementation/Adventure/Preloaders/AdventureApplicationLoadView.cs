using Modules.Localization.Scripts.Components;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Preloaders
{
    public class AdventureApplicationLoadView : ViewBase<AdventureApplicationLoadViewModel>
    {
        public static string Path = "Prefabs/Views/Adventure/Preloaders/AdventureApplicationLoadView";

        [Header("Main")]
        [SerializeField] private CachedPathImage _background;
        [SerializeField] private ProgressBar _progressBar;

        [Header("Interactable")]
        [SerializeField] private GameObject _doorImage;
        [SerializeField] private GameObject _hintHolder;
        [SerializeField] private LocalizationText _hintText;


        protected override void InitImplementation()
        {
            UpdateProgress();
            UpdateHintText();
            UpdateInteractableObjects();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;
        }

        private void OnChangeCustomHandler(string tag)
        {
            switch (tag)
            {
                case AdventureApplicationLoadViewModel.ON_CHANGE_PROGRESS:
                    UpdateProgress();
                    break;

                case AdventureApplicationLoadViewModel.ON_CHANGE_INTERACTABLE:
                    UpdateInteractableObjects();
                    break;
            }
        }

        private void UpdateProgress()
        {
            _progressBar.SetFillAmount(_viewModel.Progress);
        }

        private void UpdateInteractableObjects()
        {
            var isVisible = _viewModel.IsInteractable;

            _doorImage.SetActive(isVisible);
            _hintHolder.SetActive(isVisible);
        }

        private void UpdateHintText()
        {
            _hintText.SetText(_viewModel.HintLocalizationKey);
        }
    }
}
