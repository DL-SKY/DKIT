using Modules.Localization.Scripts.Components;
using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Hints
{
    public sealed class HintView : ViewBase<HintViewModel>
    {
        public static string Path = "Prefabs/Views/Hints/HintView";

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LocalizationText _text;
        [SerializeField] private float _showAnimationSeconds = 0.25f;
        [SerializeField] private float _hideAnimationSeconds = 0.25f;

        protected override void InitImplementation()
        {
            _text.SetText(_viewModel.LocalizationKey);
            SetCanvasGroupAlpha(_viewModel.Alpha);
        }

        protected override void Subscribe()
        {
            _viewModel.OnChange += OnChangeHandler;
            _viewModel.HideFinished += HideFinishedHandler;
        }

        protected override void Unsubscribe()
        {
            _viewModel.OnChange -= OnChangeHandler;
            _viewModel.HideFinished -= HideFinishedHandler;
        }

        public override void Show()
        {
            _viewModel.Show(_showAnimationSeconds);
        }

        public override void Hide()
        {
            _viewModel.Hide(_hideAnimationSeconds);
        }

        private void OnChangeHandler()
        {
            SetCanvasGroupAlpha(_viewModel.Alpha);
        }

        private void HideFinishedHandler()
        {
            base.Hide();
        }

        private void SetCanvasGroupAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }
    }
}
