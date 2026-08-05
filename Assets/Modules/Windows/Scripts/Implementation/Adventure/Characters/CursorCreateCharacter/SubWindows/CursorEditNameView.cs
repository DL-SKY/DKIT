using Modules.Windows.Scripts.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public class CursorEditNameView : ViewBase<CursorEditNameViewModel>
    {
        public static string Path =
            "Prefabs/Views/Adventure/Characters/CursorCreateCharacter/CursorEditNameView";

        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        protected override void InitImplementation()
        {
            if (_nameInput != null)
                _nameInput.text = _viewModel.Name;
        }

        protected override void Subscribe()
        {
            if (_nameInput != null)
                _nameInput.onValueChanged.AddListener(OnNameChanged);

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(_viewModel.OnConfirm);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(_viewModel.OnCancel);
        }

        protected override void Unsubscribe()
        {
            if (_nameInput != null)
                _nameInput.onValueChanged.RemoveListener(OnNameChanged);

            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(_viewModel.OnConfirm);

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(_viewModel.OnCancel);
        }

        private void OnNameChanged(string value)
        {
            _viewModel.SetName(value);
        }
    }
}
