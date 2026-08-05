using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public sealed class CursorAvatarListItem
    {
        public string Path;
        public string Title;
        public bool IsSelected;
    }

    public class CursorSelectAvatarViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ITEMS = "ON_CHANGE_ITEMS";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private bool _isDisposed;
        private Action<string> _onSelected;
        private readonly List<CursorAvatarListItem> _items = new List<CursorAvatarListItem>();

        public IReadOnlyList<CursorAvatarListItem> Items => _items;

        public void Init(string currentPath, Action<string> onSelected)
        {
            _isDisposed = false;
            _onSelected = onSelected;
            RebuildItems(currentPath);
        }

        public void OnSelect(string path)
        {
            if (_isDisposed || string.IsNullOrWhiteSpace(path))
                return;

            _onSelected?.Invoke(path);
            Close();
        }

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
            _onSelected = null;
            _items.Clear();
        }

        protected override Options CreateOptions()
        {
            return new Options(true, false, SortingOrderLayer.DIALOGUE);
        }

        private void RebuildItems(string currentPath)
        {
            _items.Clear();

            string unknown = _definitionsManager?.VisualSettings?.UnknownCharacterAvatar;
            if (!string.IsNullOrWhiteSpace(unknown))
            {
                _items.Add(new CursorAvatarListItem
                {
                    Path = unknown,
                    Title = "По умолчанию",
                    IsSelected = string.Equals(unknown, currentPath, StringComparison.Ordinal),
                });
            }

            // Extra placeholder option so the picker is not empty when VisualSettings is missing.
            if (_items.Count == 0)
            {
                _items.Add(new CursorAvatarListItem
                {
                    Path = currentPath ?? string.Empty,
                    Title = "Текущий",
                    IsSelected = true,
                });
            }

            SendOnChange(ON_CHANGE_ITEMS);
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }
    }
}
