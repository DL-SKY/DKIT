using Modules.Localization.Scripts.Components;
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
        [SerializeField] private LocalizationText _localText;

        protected override void InitImplementation()
        {
            _localText.SetText(_viewModel.Text);

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
