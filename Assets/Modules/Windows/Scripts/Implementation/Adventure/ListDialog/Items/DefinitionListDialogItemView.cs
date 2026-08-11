using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Items
{
    public sealed class DefinitionListDialogItemView : ListDialogItemViewBase<DefinitionListDialogItemViewModel>
    {
        public static string Path =
            "Prefabs/Views/Adventure/ListDialog/Items/DefinitionListDialogItemView";

        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;

        protected override void InitImplementation()
        {
            ApplyAll();
        }

        protected override void Subscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom += OnChangeCustomHandler;

            if (_button != null)
                _button.onClick.AddListener(OnButtonClick);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == ListDialogItemViewModelBase.ON_CHANGE_ALL)
                ApplyAll();
        }

        private void OnButtonClick()
        {
            _viewModel?.OnClick();
        }

        private void ApplyAll()
        {
            if (_viewModel == null)
                return;

            if (_title != null)
            {
                string mark = _viewModel.IsSelected ? "✓ " : string.Empty;
                _title.text = mark + _viewModel.Title;
            }

            if (_description != null)
            {
                bool hasDescription = !string.IsNullOrWhiteSpace(_viewModel.Description);
                _description.gameObject.SetActive(hasDescription);
                if (hasDescription)
                    _description.text = _viewModel.Description;
            }
        }
    }
}
