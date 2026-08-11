using System;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Items
{
    public sealed class DefinitionListDialogItemViewModel : ListDialogItemViewModelBase
    {
        private Action<string> _onSelected;

        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public void Init(
            string id,
            string title,
            string description,
            bool isSelected,
            Action<string> onSelected)
        {
            Id = id ?? string.Empty;
            Title = string.IsNullOrWhiteSpace(title) ? Id : title;
            Description = description ?? string.Empty;
            IsSelected = isSelected;
            _onSelected = onSelected;
        }

        public override void OnClick()
        {
            if (IsDisposed || string.IsNullOrWhiteSpace(Id))
                return;

            _onSelected?.Invoke(Id);
        }

        protected override void DisposeImplementation()
        {
            _onSelected = null;
            Title = string.Empty;
            Description = string.Empty;
            Id = string.Empty;
            IsSelected = false;
        }
    }
}
