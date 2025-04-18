using System;

namespace Modules.Windows.Scripts.Base
{
    public interface IViewModel : IDisposable
    {
        event Action OnChange;
        event Action<string> OnChangeCustom;
    }
}
