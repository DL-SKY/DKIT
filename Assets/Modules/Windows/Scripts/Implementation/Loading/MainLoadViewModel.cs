using Modules.Initializer.Scripts.Core;
using Modules.Windows.Scripts.Base;

namespace Modules.Windows.Scripts.Implementation.Loading
{
    public class MainLoadViewModel : ViewModelBase
    {
        public const string ON_CHANGE_PROGRESS = "ON_CHANGE_PROGRESS";

        public float Progress { get; private set; }

        private IProgressable _progressableHolder;

        public void Init(IProgressable progressableHolder)
        {
            _progressableHolder = progressableHolder;

            Subscribe();

            Progress = 0.0f;
        }

        private void Subscribe()
        {
            if (_progressableHolder != null)
                _progressableHolder.OnProgressChange += OnProgressChangeHandler;
        }

        private void Unsubscribe()
        {
            if (_progressableHolder != null)
                _progressableHolder.OnProgressChange -= OnProgressChangeHandler;
        }

        private void OnProgressChangeHandler(int currentValue, int maxValue)
        {
            Progress = currentValue * 1.0f / maxValue;
            SendOnChange(ON_CHANGE_PROGRESS);
        }

        public override void Dispose()
        {
            Unsubscribe();
        }
    }
}
