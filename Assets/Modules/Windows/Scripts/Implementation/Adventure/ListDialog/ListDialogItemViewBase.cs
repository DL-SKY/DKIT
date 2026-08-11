using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    /// <summary>
    /// Non-generic base for list-dialog item views (no Canvas — nested under the dialog shell).
    /// VM lifetime is owned by <see cref="ListDialogViewModel"/>.
    /// </summary>
    public abstract class ListDialogItemViewBase : MonoBehaviour
    {
        public ListDialogItemViewModelBase ItemViewModel { get; private set; }

        public void Init(ListDialogItemViewModelBase viewModel)
        {
            OnViewDestroy();

            ItemViewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            OnInit(viewModel);
        }

        protected abstract void OnInit(ListDialogItemViewModelBase viewModel);

        protected abstract void OnViewDestroy();

        private void OnDestroy()
        {
            OnViewDestroy();
            ItemViewModel = null;
        }
    }

    /// <summary>
    /// Typed item view paired with <typeparamref name="TViewModel"/>.
    /// </summary>
    public abstract class ListDialogItemViewBase<TViewModel> : ListDialogItemViewBase
        where TViewModel : ListDialogItemViewModelBase
    {
        protected TViewModel _viewModel;

        public TViewModel ViewModel => _viewModel;

        protected sealed override void OnInit(ListDialogItemViewModelBase viewModel)
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
