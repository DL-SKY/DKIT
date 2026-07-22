using TMPro;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Item
{
    /// <summary>
    /// Item content view (title / description). Pair with <see cref="Animation.FadeInContentAnimator"/> on the same GameObject.
    /// Icon support can be added when <c>ItemDef</c> exposes an icon path.
    /// </summary>
    public sealed class AdventureItemContentView : AdventureContentViewBase<AdventureItemContentViewModel>
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;

        protected override void InitImplementation()
        {
            if (_title != null)
                _title.text = _viewModel.Title;
            else
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureItemContentView)}] Title TextMeshProUGUI is not assigned on '{name}'.",
                    this);

            if (_description != null)
            {
                _description.text = _viewModel.Description;
                _description.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.Description));
            }
        }

        protected override void Subscribe()
        {
        }

        protected override void Unsubscribe()
        {
        }
    }
}
