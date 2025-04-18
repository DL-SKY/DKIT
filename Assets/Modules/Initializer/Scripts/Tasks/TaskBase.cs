using System;

namespace Modules.Initializer.Scripts.Tasks
{
    public abstract class TaskBase
    {
        public event Action<TaskBase> OnComplete;
        public event Action<TaskBase, int> OnError;

        public int Weight => _weight;
        private int _weight;

        protected TaskBase(int weight)
        {
            _weight = weight;
        }

        public abstract void Run();

        protected void Complete()
        {
            OnComplete?.Invoke(this);
        }

        protected void Fail(int errorCode)
        {
            OnError?.Invoke(this, errorCode);
        }
    }
}
