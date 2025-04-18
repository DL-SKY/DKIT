using System;

namespace Modules.Initializer.Scripts.Core
{
    public interface IInitializer
    {
        event Action<int, int> OnProgressChange;

        void Run(Action completedCallback, Action<int> failedCallback);
    }
}
