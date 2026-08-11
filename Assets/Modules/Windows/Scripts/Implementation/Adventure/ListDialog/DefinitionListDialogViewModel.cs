using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Items;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    public sealed class DefinitionListDialogViewModel : ListDialogViewModel
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private Action<string> _onSelected;
        private string _selectedId;

        public void Init(
            string title,
            DefinitionListDialogMode mode,
            string selectedId,
            Action<string> onSelected)
        {
            Title = title ?? string.Empty;
            Layout = ListDialogLayoutSettings.VerticalList(rowHeight: 56f, spacing: 6f);
            ItemPrefabPath = DefinitionListDialogItemView.Path;
            _selectedId = selectedId ?? string.Empty;
            _onSelected = onSelected;

            RebuildItems(mode);
            NotifyAllChanged();
        }

        protected override void DisposeImplementation()
        {
            _onSelected = null;
            _selectedId = string.Empty;
        }

        private void RebuildItems(DefinitionListDialogMode mode)
        {
            ClearItems();

            List<ItemSnapshot> snapshots = new List<ItemSnapshot>();

            switch (mode)
            {
                case DefinitionListDialogMode.Ancestry:
                    CollectAncestries(snapshots);
                    break;
                case DefinitionListDialogMode.Class:
                    CollectClasses(snapshots);
                    break;
                case DefinitionListDialogMode.Background:
                    CollectBackgrounds(snapshots);
                    break;
            }

            snapshots.Sort((a, b) =>
                string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));

            for (int i = 0; i < snapshots.Count; i++)
            {
                ItemSnapshot snapshot = snapshots[i];
                DefinitionListDialogItemViewModel item = AddItem<DefinitionListDialogItemViewModel>();
                item.Init(
                    snapshot.Id,
                    snapshot.Title,
                    snapshot.Description,
                    snapshot.IsSelected,
                    OnItemSelected);
            }
        }

        private void CollectAncestries(List<ItemSnapshot> snapshots)
        {
            if (_definitionsManager?.Ancestries == null)
                return;

            foreach (KeyValuePair<string, AncestryDef> pair in _definitionsManager.Ancestries)
            {
                AncestryDef def = pair.Value;
                if (def == null || def.Disabled)
                    continue;

                snapshots.Add(CreateSnapshot(pair.Key, def.Title, def.Description));
            }
        }

        private void CollectClasses(List<ItemSnapshot> snapshots)
        {
            if (_definitionsManager?.Classes == null)
                return;

            foreach (KeyValuePair<string, ClassDef> pair in _definitionsManager.Classes)
            {
                ClassDef def = pair.Value;
                if (def == null || def.Disabled)
                    continue;

                snapshots.Add(CreateSnapshot(pair.Key, def.Title, def.Description));
            }
        }

        private void CollectBackgrounds(List<ItemSnapshot> snapshots)
        {
            if (_definitionsManager?.Backgrounds == null)
                return;

            foreach (KeyValuePair<string, BackgroundDef> pair in _definitionsManager.Backgrounds)
            {
                BackgroundDef def = pair.Value;
                if (def == null || def.Disabled)
                    continue;

                snapshots.Add(CreateSnapshot(pair.Key, def.Title, def.Description));
            }
        }

        private ItemSnapshot CreateSnapshot(string id, string title, string description)
        {
            string resolvedTitle = string.IsNullOrWhiteSpace(title) ? id : title;
            return new ItemSnapshot(
                id,
                resolvedTitle,
                description ?? string.Empty,
                string.Equals(id, _selectedId, StringComparison.Ordinal));
        }

        private void OnItemSelected(string id)
        {
            if (IsDisposed)
                return;

            _onSelected?.Invoke(id);
            Close();
        }

        private readonly struct ItemSnapshot
        {
            public readonly string Id;
            public readonly string Title;
            public readonly string Description;
            public readonly bool IsSelected;

            public ItemSnapshot(string id, string title, string description, bool isSelected)
            {
                Id = id;
                Title = title;
                Description = description;
                IsSelected = isSelected;
            }
        }
    }
}
