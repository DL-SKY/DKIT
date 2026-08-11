using Modules.Windows.Scripts.Base;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    /// <summary>
    /// Base ViewModel for a single row/cell inside <see cref="ListDialogView"/>.
    /// Lifetime is owned by <see cref="ListDialogViewModel"/>.
    /// </summary>
    public abstract class ListDialogItemViewModelBase : ViewModelBase
    {
        public const string ON_CHANGE_ALL = "ON_CHANGE_ALL";

        private bool _isDisposed;

        public string Id { get; protected set; } = string.Empty;
        public bool IsSelected { get; protected set; }

        public bool IsDisposed => _isDisposed;

        public abstract void OnClick();

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            DisposeImplementation();
        }

        protected virtual void DisposeImplementation()
        {
        }
    }
}
