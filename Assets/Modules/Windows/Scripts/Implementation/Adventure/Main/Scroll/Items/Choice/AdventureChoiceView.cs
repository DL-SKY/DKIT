using TMPro;
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
        [SerializeField] private TextMeshProUGUI _text;

        [Header("Description")]
        [SerializeField] private TextMeshProUGUI _description;

        [Header("Button")]
        [SerializeField] private Button _button;

        protected override void InitImplementation()
        {
            if (_text != null)
                _text.text = _viewModel.Text;
            else
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureChoiceView)}] Text TextMeshProUGUI is not assigned on '{name}'.",
                    this);

            if (_description != null)
            {
                _description.text = _viewModel.Description;
                _description.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.Description));
            }

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
