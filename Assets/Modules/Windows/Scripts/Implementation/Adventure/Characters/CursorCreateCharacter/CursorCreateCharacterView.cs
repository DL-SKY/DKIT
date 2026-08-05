using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter
{
    /// <summary>
    /// Cursor character creation main screen. Prefab must live under Resources at <see cref="Path"/>.
    /// </summary>
    public class CursorCreateCharacterView : ViewBase<CursorCreateCharacterViewModel>
    {
        public static string Path =
            "Prefabs/Views/Adventure/Characters/CursorCreateCharacter/CursorCreateCharacterView";

        [Header("Avatar")]
        [SerializeField] private CachedPathImage _avatarImage;
        [SerializeField] private Button _editAvatarButton;

        [Header("Name")]
        [SerializeField] private TextMeshProUGUI _nameValue;
        [SerializeField] private Button _editNameButton;

        [Header("Ancestry")]
        [SerializeField] private TextMeshProUGUI _ancestryValue;
        [SerializeField] private Button _editAncestryButton;

        [Header("Class")]
        [SerializeField] private TextMeshProUGUI _classValue;
        [SerializeField] private Button _editClassButton;

        [Header("Background")]
        [SerializeField] private TextMeshProUGUI _backgroundValue;
        [SerializeField] private Button _editBackgroundButton;

        [Header("Abilities")]
        [SerializeField] private Button _editAbilitiesButton;
        [SerializeField] private TextMeshProUGUI _boostPointsValue;
        [SerializeField] private CachedPathImage[] _abilityIcons;
        [SerializeField] private TextMeshProUGUI[] _abilityValues;

        [Header("Skills")]
        [SerializeField] private Button _editSkillsButton;

        [Header("Footer")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _createActiveButton;
        [SerializeField] private Button _createDisableButton;

        protected override void InitImplementation()
        {
            ApplyAll();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;

            Bind(_editAvatarButton, _viewModel.OnEditAvatar);
            Bind(_editNameButton, _viewModel.OnEditName);
            Bind(_editAncestryButton, _viewModel.OnEditAncestry);
            Bind(_editClassButton, _viewModel.OnEditClass);
            Bind(_editBackgroundButton, _viewModel.OnEditBackground);
            Bind(_editAbilitiesButton, _viewModel.OnEditAbilities);
            Bind(_editSkillsButton, _viewModel.OnEditSkills);
            Bind(_closeButton, _viewModel.OnClose);
            Bind(_createActiveButton, _viewModel.OnCreateActive);
            Bind(_createDisableButton, _viewModel.OnCreateDisable);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            if (_viewModel == null)
                return;

            Unbind(_editAvatarButton, _viewModel.OnEditAvatar);
            Unbind(_editNameButton, _viewModel.OnEditName);
            Unbind(_editAncestryButton, _viewModel.OnEditAncestry);
            Unbind(_editClassButton, _viewModel.OnEditClass);
            Unbind(_editBackgroundButton, _viewModel.OnEditBackground);
            Unbind(_editAbilitiesButton, _viewModel.OnEditAbilities);
            Unbind(_editSkillsButton, _viewModel.OnEditSkills);
            Unbind(_closeButton, _viewModel.OnClose);
            Unbind(_createActiveButton, _viewModel.OnCreateActive);
            Unbind(_createDisableButton, _viewModel.OnCreateDisable);
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == CursorCreateCharacterViewModel.ON_CHANGE_ALL)
                ApplyAll();

            if (tag == CursorCreateCharacterViewModel.ON_CHANGE_CAN_CREATE)
                UpdateCreateButtonsVisibility();
        }

        private void ApplyAll()
        {
            if (_avatarImage != null)
                _avatarImage.SetPath(_viewModel.AvatarPath);

            SetText(_nameValue, _viewModel.NameText);
            SetText(_ancestryValue, _viewModel.AncestryText);
            SetText(_classValue, _viewModel.ClassText);
            SetText(_backgroundValue, _viewModel.BackgroundText);
            SetText(_boostPointsValue, $"Свободно: {_viewModel.BoostPoints}");

            ApplyAbilities();
            UpdateCreateButtonsVisibility();
        }

        private void ApplyAbilities()
        {
            var keys = _viewModel.AbilityKeys;
            int count = keys != null ? keys.Count : 0;

            for (int i = 0; i < count; i++)
            {
                string key = keys[i];

                if (_abilityIcons != null && i < _abilityIcons.Length && _abilityIcons[i] != null)
                    _abilityIcons[i].SetPath(_viewModel.GetAbilityIconPath(key));

                if (_abilityValues != null && i < _abilityValues.Length && _abilityValues[i] != null)
                    _abilityValues[i].text = FormatSigned(_viewModel.GetAbilityValue(key));
            }
        }

        private void UpdateCreateButtonsVisibility()
        {
            bool canCreate = _viewModel.CanCreate;

            if (_createActiveButton != null)
                _createActiveButton.gameObject.SetActive(canCreate);

            if (_createDisableButton != null)
                _createDisableButton.gameObject.SetActive(!canCreate);
        }

        private static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null)
                label.text = value ?? string.Empty;
        }

        private static string FormatSigned(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null && action != null)
                button.onClick.AddListener(action);
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null && action != null)
                button.onClick.RemoveListener(action);
        }
    }
}
