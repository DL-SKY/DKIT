using TMPro;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Text
{
    /// <summary>
    /// Text content item view. Pair with <see cref="Animation.TypewriterContentAnimator"/> on the same GameObject.
    /// </summary>
    public sealed class AdventureTextContentView : AdventureContentViewBase<AdventureTextContentViewModel>
    {
        [SerializeField] private TextMeshProUGUI _text;

        protected override void InitImplementation()
        {
            if (_text == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureTextContentView)}] TextMeshProUGUI is not assigned on '{name}'.",
                    this);
                return;
            }

            _text.text = _viewModel.Text;
            // Hide until TypewriterContentAnimator.Play reveals characters.
            if (Animator != null)
                _text.maxVisibleCharacters = 0;
        }

        protected override void Subscribe()
        {
        }

        protected override void Unsubscribe()
        {
        }
    }
}
