using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System.Collections.Generic;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    /// <summary>
    /// Base ViewModel for the shared list/picker dialog shell.
    /// Concrete subclasses fill <see cref="Items"/>, <see cref="Layout"/> and <see cref="ItemPrefabPath"/>.
    /// </summary>
    public abstract class ListDialogViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ALL = "ON_CHANGE_ALL";
        public const string ON_CHANGE_ITEMS = "ON_CHANGE_ITEMS";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        private readonly List<ListDialogItemViewModelBase> _items = new List<ListDialogItemViewModelBase>();
        private bool _isDisposed;
        
        public ListDialogLayoutSettings Layout { get; protected set; } = ListDialogLayoutSettings.VerticalList();
        public string ItemPrefabPath { get; protected set; } = string.Empty;

        public string Title { get; protected set; } = string.Empty;

        public IReadOnlyList<ListDialogItemViewModelBase> Items => _items;

        public bool IsDisposed => _isDisposed;

        public void OnCancel()
        {
            if (_isDisposed)
                return;

            Close();
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            ClearItems();
            Title = string.Empty;
            ItemPrefabPath = string.Empty;
            Layout = null;
            DisposeImplementation();
        }

        protected virtual void DisposeImplementation()
        {
        }

        protected TItem AddItem<TItem>() where TItem : ListDialogItemViewModelBase
        {
            TItem item = _viewModelFactory.Create<TItem>();
            _items.Add(item);
            return item;
        }

        protected void ClearItems()
        {
            for (int i = 0; i < _items.Count; i++)
                _items[i]?.Dispose();

            _items.Clear();
        }

        protected void NotifyItemsChanged()
        {
            SendOnChange(ON_CHANGE_ITEMS);
        }

        protected void NotifyAllChanged()
        {
            SendOnChange(ON_CHANGE_ALL);
        }

        protected void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }

        protected override Options CreateOptions()
        {
            return new Options(true, false, SortingOrderLayer.DIALOGUE);
        }
    }
}
