using Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice
{
    /// <summary>
    /// Non-generic base for adventure choice panel views (prefab references, sequencer).
    /// VM lifetime is owned by <see cref="AdventureScrollViewModel"/> — this view only unsubscribes on destroy.
    /// Expects an <see cref="IContentAnimator"/> on the same GameObject.
    /// </summary>
    public abstract class AdventureChoiceViewBase : MonoBehaviour
    {
        public IContentAnimator Animator { get; private set; }

        public AdventureChoiceViewModelBase ChoiceViewModel { get; private set; }

        public void Init(AdventureChoiceViewModelBase viewModel)
        {
            ChoiceViewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            Animator = GetComponent<IContentAnimator>();

            if (Animator == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{GetType().Name}] No {nameof(IContentAnimator)} on '{name}'. Appearance animation will be skipped.",
                    this);
            }

            OnInit(viewModel);
        }

        protected abstract void OnInit(AdventureChoiceViewModelBase viewModel);

        private void OnDestroy()
        {
            OnViewDestroy();
            ChoiceViewModel = null;
            Animator = null;
        }

        protected abstract void OnViewDestroy();
    }

    /// <summary>
    /// Typed choice panel view. Pair with a matching <typeparamref name="TViewModel"/>.
    /// </summary>
    public abstract class AdventureChoiceViewBase<TViewModel> : AdventureChoiceViewBase
        where TViewModel : AdventureChoiceViewModelBase
    {
        protected TViewModel _viewModel;

        public TViewModel ViewModel => _viewModel;

        protected sealed override void OnInit(AdventureChoiceViewModelBase viewModel)
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
