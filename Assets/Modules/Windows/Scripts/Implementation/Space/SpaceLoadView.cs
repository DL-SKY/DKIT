using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    public sealed class SpaceLoadView : ViewBase<SpaceLoadViewModel>
    {
        public static string Path = "Prefabs/Views/Space/SpaceLoadView";

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
            if (tag == SpaceLoadViewModel.ON_CHANGE_PROGRESS)
                UpdateProgress();
        }

        private void UpdateProgress()
        {
            _progressBar.SetFillAmount(_viewModel.Progress);
        }
    }
}
