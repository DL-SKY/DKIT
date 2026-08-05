using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public class CursorSelectDefinitionViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ITEMS = "ON_CHANGE_ITEMS";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private bool _isDisposed;
        private Action<string> _onSelected;
        private string _selectedId;
        private readonly List<CursorDefinitionListItem> _items = new List<CursorDefinitionListItem>();

        public string Title { get; private set; } = string.Empty;
        public IReadOnlyList<CursorDefinitionListItem> Items => _items;

        public void Init(
            string title,
            CursorDefinitionSelectMode mode,
            string selectedId,
            Action<string> onSelected)
        {
            _isDisposed = false;
            Title = title ?? string.Empty;
            _selectedId = selectedId ?? string.Empty;
            _onSelected = onSelected;
            RebuildItems(mode);
        }

        public void OnSelectItem(string id)
        {
            if (_isDisposed || string.IsNullOrWhiteSpace(id))
                return;

            _onSelected?.Invoke(id);
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
            Title = string.Empty;
            _selectedId = string.Empty;
        }

        protected override Options CreateOptions()
        {
            return new Options(true, false, SortingOrderLayer.DIALOGUE);
        }

        private void RebuildItems(CursorDefinitionSelectMode mode)
        {
            _items.Clear();

            switch (mode)
            {
                case CursorDefinitionSelectMode.Ancestry:
                    FillAncestries();
                    break;
                case CursorDefinitionSelectMode.Class:
                    FillClasses();
                    break;
                case CursorDefinitionSelectMode.Background:
                    FillBackgrounds();
                    break;
            }

            SendOnChange(ON_CHANGE_ITEMS);
        }

        private void FillAncestries()
        {
            if (_definitionsManager?.Ancestries == null)
                return;

            foreach (KeyValuePair<string, AncestryDef> pair in _definitionsManager.Ancestries)
            {
                AncestryDef def = pair.Value;
                if (def == null || def.Disabled)
                    continue;

                _items.Add(CreateItem(pair.Key, def.Title, def.Description));
            }

            SortItems();
        }

        private void FillClasses()
        {
            if (_definitionsManager?.Classes == null)
                return;

            foreach (KeyValuePair<string, ClassDef> pair in _definitionsManager.Classes)
            {
                ClassDef def = pair.Value;
                if (def == null || def.Disabled)
                    continue;

                _items.Add(CreateItem(pair.Key, def.Title, def.Description));
            }

            SortItems();
        }

        private void FillBackgrounds()
        {
            if (_definitionsManager?.Backgrounds == null)
                return;

            foreach (KeyValuePair<string, BackgroundDef> pair in _definitionsManager.Backgrounds)
            {
                BackgroundDef def = pair.Value;
                if (def == null || def.Disabled)
                    continue;

                _items.Add(CreateItem(pair.Key, def.Title, def.Description));
            }

            SortItems();
        }

        private CursorDefinitionListItem CreateItem(string id, string title, string description)
        {
            return new CursorDefinitionListItem
            {
                Id = id,
                Title = string.IsNullOrWhiteSpace(title) ? id : title,
                Description = description ?? string.Empty,
                IsSelected = string.Equals(id, _selectedId, StringComparison.Ordinal),
            };
        }

        private void SortItems()
        {
            _items.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }
    }
}
