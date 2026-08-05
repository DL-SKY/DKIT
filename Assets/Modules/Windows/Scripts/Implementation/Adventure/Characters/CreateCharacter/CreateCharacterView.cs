using Modules.Localization.Scripts.Components;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using TMPro;
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

        [Header("Name")]
        [SerializeField] private TextMeshProUGUI _nameValue;
        [SerializeField] private Button _nameButton;

        [Header("Avatar")]
        [SerializeField] private CachedPathImage _avatarImage;
        [SerializeField] private Button _avatarButton;

        [Header("Hit Points")]
        [SerializeField] private LocalizationText _hitPointsTitle;
        [SerializeField] private TextMeshProUGUI _hitPointsValue;

        [Header("Level")]
        [SerializeField] private LocalizationText _levelTitle;
        [SerializeField] private TextMeshProUGUI _levelValue;

        [Header("Ancestry")]
        [SerializeField] private CachedPathImage _ancestryIcon;
        [SerializeField] private LocalizationText _ancestryText;
        [SerializeField] private Button _ancestryButton;

        [Header("Class")]
        [SerializeField] private CachedPathImage _classIcon;
        [SerializeField] private LocalizationText _classText;
        [SerializeField] private Button _classButton;

        [Header("Background")]
        [SerializeField] private CachedPathImage _backgroundIcon;
        [SerializeField] private LocalizationText _backgroundText;
        [SerializeField] private Button _backgroundButton;

        [Header("Ability Scores")]
        [SerializeField] private LocalizationText _abilityScoresTitle;
        [SerializeField] private LocalizationText _abilityBoostPointsTitle;
        [SerializeField] private TextMeshProUGUI _abilityBoostPointsValue;
        [SerializeField] private Button _abilityScoresButton;

        [Header("Skills")]
        [SerializeField] private LocalizationText _skillsTitle;
        [SerializeField] private LocalizationText _skillsBoostPointsTitle;
        [SerializeField] private TextMeshProUGUI _skillsBoostPointsValue;
        [SerializeField] private Button _skillsButton;

        protected override void InitImplementation()
        {
            UpdateCharacter();
            UpdateCreateButtonsVisibility();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;

            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _createActiveButton.onClick.AddListener(OnCreateActiveButtonClick);
            _createDisableButton.onClick.AddListener(OnCreateDisableButtonClick);

            _nameButton.onClick.AddListener(OnNameButtonClick);
            _avatarButton.onClick.AddListener(OnAvatarButtonClick);
            _ancestryButton.onClick.AddListener(OnAncestryButtonClick);
            _classButton.onClick.AddListener(OnClassButtonClick);
            _backgroundButton.onClick.AddListener(OnBackgroundButtonClick);
            _abilityScoresButton.onClick.AddListener(OnAbilityScoresButtonClick);
            _skillsButton.onClick.AddListener(OnSkillsButtonClick);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
            _createActiveButton.onClick.RemoveListener(OnCreateActiveButtonClick);
            _createDisableButton.onClick.RemoveListener(OnCreateDisableButtonClick);

            _nameButton.onClick.RemoveListener(OnNameButtonClick);
            _avatarButton.onClick.RemoveListener(OnAvatarButtonClick);
            _ancestryButton.onClick.RemoveListener(OnAncestryButtonClick);
            _classButton.onClick.RemoveListener(OnClassButtonClick);
            _backgroundButton.onClick.RemoveListener(OnBackgroundButtonClick);
            _abilityScoresButton.onClick.RemoveListener(OnAbilityScoresButtonClick);
            _skillsButton.onClick.RemoveListener(OnSkillsButtonClick);
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == CreateCharacterViewModel.ON_CHANGE_CHARACTER)
                UpdateCharacter();

            if (tag == CreateCharacterViewModel.ON_CHANGE_CAN_CREATE)
                UpdateCreateButtonsVisibility();
        }

        private void UpdateCharacter()
        {
            //TODO: ...
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

        private void OnNameButtonClick()
        {
            _viewModel.OnEditName();
        }

        private void OnAvatarButtonClick()
        {
            _viewModel.OnEditAvatar();
        }

        private void OnAncestryButtonClick()
        {
            _viewModel.OnEditAncestry();
        }

        private void OnClassButtonClick()
        {
            _viewModel.OnEditClass();
        }

        private void OnBackgroundButtonClick()
        {
            _viewModel.OnEditBackground();
        }

        private void OnAbilityScoresButtonClick()
        {
            _viewModel.OnEditAbilities();
        }

        private void OnSkillsButtonClick()
        {
            _viewModel.OnEditSkills();
        }
    }
}
