using Modules.Windows.Scripts.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CreateCharacter
{
    /// <summary>
    /// Character creation screen. Prefab must live under Resources at <see cref="Path"/>.
    /// </summary>
    public class CreateCharacterView : ViewBase<CreateCharacterViewModel>
    {
        //TODO: create prefab view!!!
        public static string Path = "";

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _createActiveButton;
        [SerializeField] private Button _createDisableButton;

        protected override void InitImplementation()
        {
            UpdateCreateButtonsVisibility();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;

            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _createActiveButton.onClick.AddListener(OnCreateActiveButtonClick);
            _createDisableButton.onClick.AddListener(OnCreateDisableButtonClick);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
            _createActiveButton.onClick.RemoveListener(OnCreateActiveButtonClick);
            _createDisableButton.onClick.RemoveListener(OnCreateDisableButtonClick);
        }

        public override void Show()
        {
            base.Show();
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == CreateCharacterViewModel.ON_CHANGE_CAN_CREATE)
                UpdateCreateButtonsVisibility();
        }

        private void UpdateCreateButtonsVisibility()
        {
            bool canCreate = _viewModel.CanCreate;
            _createActiveButton.gameObject.SetActive(canCreate);
            _createDisableButton.gameObject.SetActive(!canCreate);
        }

        private void OnCloseButtonClick()
        {
            _viewModel.OnClose();
        }

        private void OnCreateActiveButtonClick()
        {
            _viewModel.OnCreateActive();
        }

        private void OnCreateDisableButtonClick()
        {
            _viewModel.OnCreateDisable();
        }
    }
}
