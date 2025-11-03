using System;

namespace Modules.Initializer.Scripts.Core
{
    public interface IInitializer : IProgressable
    {
        void Run(Action completedCallback, Action<int> failedCallback);
    }
}
