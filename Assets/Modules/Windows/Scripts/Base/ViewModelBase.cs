using Modules.Windows.Scripts.Settings;
using System;

namespace Modules.Windows.Scripts.Base
{
    public abstract class ViewModelBase : IViewModel
    {
        public event Action OnChange;
        public event Action<string> OnChangeCustom;

        public readonly Options Options;

        public int ViewHandle => _viewHandle;
        protected int _viewHandle;

        protected ViewModelBase()
        {
            Options = CreateOptions();
        }

        protected virtual Options CreateOptions()
        {
            return new Options();
        }

        protected void SendOnChange()
        {
            OnChange?.Invoke();
        }

        protected void SendOnChange(string tag)
        {
            OnChangeCustom?.Invoke(tag);
        }

        public void SetHandle(int handle)
        {
            _viewHandle = handle;
        }

        public abstract void Dispose();
    }
}
