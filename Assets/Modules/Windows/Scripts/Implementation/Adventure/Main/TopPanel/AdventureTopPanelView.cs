using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    /// <summary>
    /// Sub-view for the adventure top panel; attach to a child object inside <c>AdventureMainView</c> prefab
    /// (e.g. the former TopFrame). Lifetime of the VM is owned by <see cref="AdventureMainViewModel"/>.
    /// </summary>
    public class AdventureTopPanelView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _characterButton;
        [SerializeField] private Button _menuButton;

        [Header("Abilities")]
        [SerializeField] private List<AdventureParameterView> _parameterViews;

        private AdventureTopPanelViewModel _viewModel;

        public void Init(AdventureTopPanelViewModel viewModel)
        {
            Unsubscribe();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            InitParameterViews();
            Subscribe();
        }

        private void InitParameterViews()
        {
            if (_parameterViews == null || _parameterViews.Count == 0)
                return;

            for (int i = 0; i < _parameterViews.Count; i++)
            {
                var parameterView = _parameterViews[i];
                var parameterViewModel = _viewModel.CreateParameter();
                parameterView.Init(parameterViewModel);
            }
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
