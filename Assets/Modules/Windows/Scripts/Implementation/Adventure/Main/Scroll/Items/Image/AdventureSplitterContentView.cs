using TMPro;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// Decorative splitter panel. Pair with <see cref="Animation.FadeInContentAnimator"/> on the same GameObject.
    /// Image is static in the prefab — no path loading. Animator uses <c>CanvasGroup</c> only.
    /// </summary>
    public sealed class AdventureSplitterContentView : AdventureContentViewBase<AdventureSplitterContentViewModel>
    {
        [SerializeField] private TextMeshProUGUI _debugText;

        protected override void InitImplementation()
        {
            if (_debugText != null)
                _debugText.text = _viewModel.DebugText;
        }

        protected override void Subscribe()
        {
        }

        protected override void Unsubscribe()
        {
        }
    }
}
