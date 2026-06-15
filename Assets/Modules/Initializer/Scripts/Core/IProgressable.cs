using System;

namespace Modules.Initializer.Scripts.Core
{
    public interface IProgressable
    {
        /// <summary>
        /// Action(current, max)
        /// </summary>
        event Action<int, int> OnProgressChange;
    }
}
