using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items
{
    /// <summary>
    /// Non-generic base for adventure content item views (prefab references, sequencer).
    /// VM lifetime is owned by <see cref="AdventureScrollViewModel"/> — this view only unsubscribes on destroy.
    /// Expects an <see cref="IContentAnimator"/> on the same GameObject.
    /// </summary>
    public abstract class AdventureContentViewBase : MonoBehaviour
    {
        public IContentAnimator Animator { get; private set; }

        public AdventureContentViewModelBase ContentViewModel { get; private set; }

        public void Init(AdventureContentViewModelBase viewModel)
        {
            ContentViewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            Animator = GetComponent<IContentAnimator>();

            if (Animator == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{GetType().Name}] No {nameof(IContentAnimator)} on '{name}'. Appearance animation will be skipped.",
                    this);
            }

            OnInit(viewModel);
        }

        protected abstract void OnInit(AdventureContentViewModelBase viewModel);

        private void OnDestroy()
        {
            OnViewDestroy();
            ContentViewModel = null;
            Animator = null;
        }

        protected abstract void OnViewDestroy();
    }

    /// <summary>
    /// Typed content item view. Pair with a matching <typeparamref name="TViewModel"/>.
    /// </summary>
    public abstract class AdventureContentViewBase<TViewModel> : AdventureContentViewBase
        where TViewModel : AdventureContentViewModelBase
    {
        protected TViewModel _viewModel;

        public TViewModel ViewModel => _viewModel;

        protected sealed override void OnInit(AdventureContentViewModelBase viewModel)
        {
            _viewModel = (TViewModel)viewModel;
            Subscribe();
            InitImplementation();
        }

        protected abstract void InitImplementation();

        protected abstract void Subscribe();

        protected abstract void Unsubscribe();

        protected sealed override void OnViewDestroy()
        {
            Unsubscribe();
            _viewModel = null;
        }
    }
}
