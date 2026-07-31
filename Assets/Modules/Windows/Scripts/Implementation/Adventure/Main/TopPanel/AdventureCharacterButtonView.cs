using Modules.Windows.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    /// <summary>
    /// Sub-view for the active-character button inside <see cref="AdventureTopPanelView"/>.
    /// Lifetime of the VM is owned by <see cref="AdventureTopPanelViewModel"/>.
    /// </summary>
    public class AdventureCharacterButtonView : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private Button _button;
        [SerializeField] private CachedPathImage _icon;

        private AdventureCharacterButtonViewModel _viewModel;

        public void Init(AdventureCharacterButtonViewModel viewModel)
        {
            Unsubscribe();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            viewModel.Init();

            ApplyAll();
            Subscribe();
        }

        private void Subscribe()
        {
            if (_viewModel == null)
                return;

            _viewModel.OnChangeCustom += OnChangeCustomHandler;
            _button.onClick.AddListener(OnButtonClick);
        }

        private void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == AdventureCharacterButtonViewModel.ON_CHANGE_ICON)
                ApplyIcon();
        }

        private void OnButtonClick()
        {
            _viewModel?.OnClick();
        }

        private void ApplyAll()
        {
            ApplyIcon();
        }

        private void ApplyIcon()
        {
            if (_icon == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureCharacterButtonView)}] CachedPathImage is not assigned on '{name}'.",
                    this);
                return;
            }

            _icon.SetPath(_viewModel.IconPath);
        }

        private void OnDestroy()
        {
            Unsubscribe();
            _viewModel = null;
        }
    }
}
