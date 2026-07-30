using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.BottomPanel
{
    /// <summary>
    /// Sub-view for the adventure bottom panel; attach to a child object inside <c>AdventureMainView</c> prefab
    /// (e.g. the former BottomFrame). Lifetime of the VM is owned by <see cref="AdventureMainViewModel"/>.
    /// </summary>
    public class AdventureBottomPanelView : MonoBehaviour
    {
        private AdventureBottomPanelViewModel _viewModel;

        public void Init(AdventureBottomPanelViewModel viewModel)
        {
            Unsubscribe();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            Subscribe();
        }

        private void Subscribe()
        {
            if (_viewModel == null)
                return;

            _viewModel.OnChangeCustom += OnChangeCustomHandler;
        }

        private void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;
        }

        private void OnChangeCustomHandler(string tag)
        {
        }

        private void OnDestroy()
        {
            Unsubscribe();
            _viewModel = null;
        }
    }
}
