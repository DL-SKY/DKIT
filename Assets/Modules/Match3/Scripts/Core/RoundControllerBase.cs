using Modules.Match3.Scripts.Interfaces;
using System;

namespace Modules.Match3.Scripts.Core
{
    public abstract class RoundControllerBase : IDisposable
    {
        protected IGameRoundData _data;

        protected void InitBase(IGameRoundData data)
        {
            _data = data;

            Subscribe();
            InitImplementation();
        }

        public abstract void Dispose();

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract void InitImplementation();
    }
}
