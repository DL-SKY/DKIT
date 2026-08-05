using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public class CursorEditNameViewModel : ViewModelBase
    {
        public const string ON_CHANGE_NAME = "ON_CHANGE_NAME";

        [Inject] private readonly WindowsManager _windowsManager;

        private bool _isDisposed;
        private Action<string> _onConfirm;

        public string Name { get; private set; } = string.Empty;

        public void Init(string currentName, Action<string> onConfirm)
        {
            _isDisposed = false;
            _onConfirm = onConfirm;
            Name = currentName ?? string.Empty;
        }

        public void SetName(string value)
        {
            if (_isDisposed)
                return;

            Name = value ?? string.Empty;
            SendOnChange(ON_CHANGE_NAME);
        }

        public void OnConfirm()
        {
            if (_isDisposed)
                return;

            string trimmed = (Name ?? string.Empty).Trim();
            _onConfirm?.Invoke(trimmed);
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
            _onConfirm = null;
            Name = string.Empty;
        }

        protected override Options CreateOptions()
        {
            return new Options(true, false, SortingOrderLayer.DIALOGUE);
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }
    }
}
