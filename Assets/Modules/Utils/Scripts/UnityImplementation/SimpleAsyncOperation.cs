using System;
using UnityEngine;

namespace Modules.Utils.Scripts.UnityImplementation
{
    public class SimpleAsyncOperation : YieldInstruction
    {
        public event Action OnCompleted;

        public bool IsDone { get; private set; }
        public float Progress { get; private set; }
        public int Priority { get; private set; }

        public SimpleAsyncOperation() : base()
        {
            IsDone = false;
            Progress = 0.0f;
            Priority = 1;
        }

        public void SetProgress(float normalizeProgress)
        {
            Progress = normalizeProgress;
        }

        public void SetCompleted()
        {
            SetProgress(1.0f);
            IsDone = true;
            OnCompleted?.Invoke();
        }
    }
}
