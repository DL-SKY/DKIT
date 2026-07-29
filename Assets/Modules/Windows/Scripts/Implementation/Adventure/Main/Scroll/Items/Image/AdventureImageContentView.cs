using Modules.Windows.Scripts.Components;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// Shared view for Image / RandomImage / Slideshow content.
    /// Pair with <see cref="Animation.FadeInContentAnimator"/> on the same GameObject.
    /// Forwards <see cref="AdventureImageContentViewModelBase.CurrentPath"/> to <see cref="CachedPathImage"/>.
    /// </summary>
    public sealed class AdventureImageContentView : AdventureContentViewBase<AdventureImageContentViewModelBase>
    {
        [SerializeField] private CachedPathImage _cachedPathImage;

        protected override void InitImplementation()
        {
            ApplyCurrentPath();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == AdventureImageContentViewModelBase.ON_CHANGE_PATH)
                ApplyCurrentPath();
        }

        private void ApplyCurrentPath()
        {
            _cachedPathImage.SetPath(_viewModel.CurrentPath);
        }
    }
}
