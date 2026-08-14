using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Items;
using System;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    public sealed class CursorAvatarListDialogViewModel : ListDialogViewModel
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private Action<string> _onSelected;

        public void Init(string currentPath, Action<string> onSelected)
        {
            _onSelected = onSelected;
            Title = "Аватар";
            Layout = ListDialogLayoutSettings.AvatarGrid();
            ItemPrefabPath = AvatarListDialogItemView.Path;

            RebuildItems(currentPath);
            NotifyAllChanged();
        }

        protected override void DisposeImplementation()
        {
            _onSelected = null;
        }

        private void RebuildItems(string currentPath)
        {
            ClearItems();

            string unknown = _definitionsManager?.VisualSettings?.UnknownCharacterAvatar;
            if (!string.IsNullOrWhiteSpace(unknown))
            {
                AvatarListDialogItemViewModel item = AddItem<AvatarListDialogItemViewModel>();
                item.Init(
                    avatarPath: unknown,
                    title: "По умолчанию",
                    isSelected: string.Equals(unknown, currentPath, StringComparison.Ordinal),
                    onSelected: OnItemSelected);
            }

            if (Items.Count == 0)
            {
                AvatarListDialogItemViewModel item = AddItem<AvatarListDialogItemViewModel>();
                item.Init(
                    avatarPath: currentPath ?? string.Empty,
                    title: "Текущий",
                    isSelected: true,
                    onSelected: OnItemSelected);
            }
        }

        private void OnItemSelected(string avatarPath)
        {
            if (IsDisposed)
                return;

            _onSelected?.Invoke(avatarPath);
            Close();
        }
    }
}
