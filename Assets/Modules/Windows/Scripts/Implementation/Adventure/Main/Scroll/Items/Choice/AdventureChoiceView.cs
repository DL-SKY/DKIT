using Modules.Localization.Scripts.Components;
using Modules.Windows.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice
{
    /// <summary>
    /// Choice panel view (title / description / button). Pair with <see cref="Animation.FadeInContentAnimator"/> on the same GameObject.
    /// </summary>
    public sealed class AdventureChoiceView : AdventureChoiceViewBase<AdventureChoiceViewModel>
    {
        [Header("Title")]
        [SerializeField] private LocalizationText _localText;
        [SerializeField] private CachedPathImage _mainIcon;

        [Header("Description")]
        [SerializeField] private GameObject _descriptionHolder;
        [SerializeField] private LocalizationText _localDescription;
        [SerializeField] private CachedPathImage _descriptionIcon;

        [Header("Button")]
        [SerializeField] private Button _button;

        protected override void InitImplementation()
        {
            _localText?.SetText(_viewModel.Text);
            _mainIcon?.SetPath(_viewModel.MainIcon);

            _descriptionHolder?.SetActive(_viewModel.EnabledDescription);
            _localDescription?.SetText(_viewModel.Description, _viewModel.DescriptionParam);
            _descriptionIcon?.SetPath(_viewModel.DescriptionIcon);

            if (_button == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureChoiceView)}] Button is not assigned on '{name}'.",
                    this);
            }
        }

        protected override void Subscribe()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        protected override void Unsubscribe()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _viewModel?.Select();
        }
    }
}
