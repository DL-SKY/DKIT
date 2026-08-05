using Modules.Windows.Scripts.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    /// <summary>
    /// Sub-view for a single parameter slot inside <see cref="AdventureTopPanelView"/>
    /// (e.g. AbilityButton under Abilities). Lifetime of the VM is owned by
    /// <see cref="AdventureTopPanelViewModel"/>.
    /// </summary>
    public class AdventureParameterView : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _parameterName;

        [Header("Links")]
        [SerializeField] private Button _button;
        [SerializeField] private CachedPathImage _icon;
        [SerializeField] private TextMeshProUGUI _value;

        private AdventureParameterViewModelBase _viewModel;

        public void Init(AdventureParameterViewModelBase viewModel)
        {
            Unsubscribe();

            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            viewModel.Init(_parameterName);

            ApplyAll();
            Subscribe();
        }

        private void Subscribe()
        {
            if (_viewModel == null)
                return;

            _viewModel.OnChangeCustom += OnChangeCustomHandler;

            if (_button != null)
                _button.onClick.AddListener(OnButtonClick);
            else
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureParameterView)}] Button is not assigned on '{name}'.",
                    this);
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
            if (tag == AdventureParameterViewModelBase.ON_CHANGE_ICON)
                ApplyIcon();

            if (tag == AdventureParameterViewModelBase.ON_CHANGE_VALUE)
                ApplyValue();
        }

        private void OnButtonClick()
        {
            _viewModel?.OnClick();
        }

        private void ApplyAll()
        {
            ApplyIcon();
            ApplyValue();
        }

        private void ApplyIcon()
        {
            if (_icon == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureParameterView)}] CachedPathImage is not assigned on '{name}'.",
                    this);
                return;
            }

            _icon.SetPath(_viewModel.IconPath);
        }

        private void ApplyValue()
        {
            if (_value == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureParameterView)}] TextMeshProUGUI is not assigned on '{name}'.",
                    this);
                return;
            }

            _value.text = _viewModel.Value;
        }

        private void OnDestroy()
        {
            Unsubscribe();
            _viewModel = null;
        }
    }
}
