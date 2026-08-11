using System;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Items
{
    public sealed class AvatarListDialogItemViewModel : ListDialogItemViewModelBase
    {
        private Action<string> _onSelected;

        public string AvatarPath { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;

        public void Init(
            string avatarPath,
            string title,
            bool isSelected,
            Action<string> onSelected)
        {
            AvatarPath = avatarPath ?? string.Empty;
            Id = AvatarPath;
            Title = title ?? string.Empty;
            IsSelected = isSelected;
            _onSelected = onSelected;
        }

        public override void OnClick()
        {
            if (IsDisposed || string.IsNullOrWhiteSpace(AvatarPath))
                return;

            _onSelected?.Invoke(AvatarPath);
        }

        protected override void DisposeImplementation()
        {
            _onSelected = null;
            AvatarPath = string.Empty;
            Title = string.Empty;
            Id = string.Empty;
            IsSelected = false;
        }
    }
}
