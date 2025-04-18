using Modules.Windows.Scripts.Settings;
using System;

namespace Modules.Windows.Scripts.Base
{
    public abstract class ViewModelBase : IViewModel
    {
        public event Action OnChange;
        public event Action<string> OnChangeCustom;

        public readonly Options Options;

        protected int _viewHandle;

        protected ViewModelBase()
        {
            Options = CreateOptions();
        }

        protected virtual Options CreateOptions()
        {
            return new Options();
        }

        public void SetHandle(int handle)
        {
            _viewHandle = handle;
        }

        public abstract void Dispose();
    }
}
