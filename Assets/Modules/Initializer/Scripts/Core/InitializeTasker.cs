using Modules.Initializer.Scripts.Tasks;
using System;
using System.Collections.Generic;

namespace Modules.Initializer.Scripts.Core
{
    public sealed class InitializeTasker : IInitializer, IDisposable
    {
        public event Action<int, int> OnProgressChange;

        private List<TaskBase> _tasks;
        private int _currentTaskIndex;

        private Action _completedCallback;
        private Action<int> _failedCallback;

        private int _currentProgress;
        private int _maxProgress;

        public InitializeTasker(List<TaskBase> tasks)
        {
            _tasks = tasks;
        }

        public void Run(Action completedCallback, Action<int> failedCallback)
        {
            _completedCallback = completedCallback;
            _failedCallback = failedCallback;

            _currentTaskIndex = 0;
            _currentProgress = 0;
            _maxProgress = 0;
            foreach (var task in _tasks)
                _maxProgress += task.Weight;

            TryStartTask();
        }

        private void TryStartTask()
        {
            if (_tasks.Count > _currentTaskIndex)
            {
                var task = _tasks[_currentTaskIndex];
                task.OnComplete += OnCompleteHandler;
                task.OnError += OnErrorHandler;

                task.Run();
            }
            else
            {
                _completedCallback?.Invoke();
            }
        }

        private void OnCompleteHandler(TaskBase task)
        {
            task.OnComplete -= OnCompleteHandler;
            task.OnError -= OnErrorHandler;

            _currentTaskIndex++;
            _currentProgress += task.Weight;
            OnProgressChange?.Invoke(_currentProgress, _maxProgress);

            TryStartTask();
        }

        private void OnErrorHandler(TaskBase task, int errorCode)
        {
            task.OnComplete -= OnCompleteHandler;
            task.OnError -= OnErrorHandler;

            _failedCallback?.Invoke(errorCode);
        }

        public void Dispose()
        {
            if (_tasks.Count > _currentTaskIndex)
            {
                var task = _tasks[_currentTaskIndex];
                task.OnComplete -= OnCompleteHandler;
                task.OnError -= OnErrorHandler;
            }
        }
    }
}
