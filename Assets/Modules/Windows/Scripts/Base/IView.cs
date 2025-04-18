using Modules.Windows.Scripts.Settings;
using System;

namespace Modules.Windows.Scripts.Base
{
    public interface IView
    {
        event Action<int> OnViewDestroy;

        int Handle { get; }
        Options Options { get; }

        void SetSortingOrder(int value);
        void Show();
        void Hide();
    }
}
