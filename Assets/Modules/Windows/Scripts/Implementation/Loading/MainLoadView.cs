using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Loading
{
    public class MainLoadView : ViewBase<MainLoadViewModel>
    {
        public static string Path = "Prefabs/Views/Loading/MainLoadView";

        [SerializeField] private ProgressBar _progressBar;

        protected override void InitImplementation()
        {
            UpdateProgress();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;
        }

        protected override void Unsubscribe()
        {
            _viewModel.OnChangeCustom -= OnChangeCustomHandler;
        }

        private void OnChangeCustomHandler(string tag)
        {
            switch (tag)
            {
                case MainLoadViewModel.ON_CHANGE_PROGRESS:
                    UpdateProgress();
                    break;
            }
        }

        private void UpdateProgress()
        {
            _progressBar.SetFillAmount(_viewModel.Progress);
        }
    }
}
