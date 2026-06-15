using System;

namespace Modules.Initializer.Scripts.Tasks
{
    /// <summary>
    /// Don't forget call Complete() and/or Fail() in Run()
    /// </summary>
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
            UnityEngine.Debug.Log($"[Task] {GetType().Name} complete.");

            OnComplete?.Invoke(this);
        }

        protected void Fail(int errorCode)
        {
            UnityEngine.Debug.LogError($"[Task] {GetType().Name} fail with error: {errorCode}!");

            OnError?.Invoke(this, errorCode);
        }
    }
}
