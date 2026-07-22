using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items
{
    /// <summary>
    /// Base MonoBehaviour view for a single adventure content item (not <see cref="Base.ViewBase{T}"/>).
    /// Expects an <see cref="IContentAnimator"/> on the same GameObject.
    /// </summary>
    public abstract class AdventureContentViewBase<TViewModel> : MonoBehaviour
        where TViewModel : AdventureContentViewModelBase
    {
        protected TViewModel _viewModel;

        public IContentAnimator Animator { get; private set; }

        public TViewModel ViewModel => _viewModel;

        public void Init(TViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            Animator = GetComponent<IContentAnimator>();

            if (Animator == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{GetType().Name}] No {nameof(IContentAnimator)} on '{name}'. Appearance animation will be skipped.",
                    this);
            }

            Subscribe();
            InitImplementation();
        }

        protected abstract void InitImplementation();

        protected abstract void Subscribe();

        protected abstract void Unsubscribe();

        private void OnDestroy()
        {
            Unsubscribe();
            _viewModel?.Dispose();
            _viewModel = null;
        }
    }
}
