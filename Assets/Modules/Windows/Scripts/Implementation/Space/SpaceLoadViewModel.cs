using Modules.Initializer.Scripts.Core;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Settings;

namespace Modules.Windows.Scripts.Implementation.Space
{
    public sealed class SpaceLoadViewModel : ViewModelBase
    {
        public const string ON_CHANGE_PROGRESS = "ON_CHANGE_PROGRESS";

        public float Progress { get; private set; }

        private IProgressable _progressableHolder;

        public void Init(IProgressable progressableHolder)
        {
            _progressableHolder = progressableHolder;
            Progress = 0.0f;
            Subscribe();
        }

        protected override Options CreateOptions()
        {
            return new Options(canCloseOnEsc: false, hideInHistory: false, sortingLayer: SortingOrderLayer.PRELOADER);
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
            Progress = maxValue > 0 ? currentValue * 1.0f / maxValue : 0.0f;
            SendOnChange(ON_CHANGE_PROGRESS);
        }

        public override void Dispose()
        {
            Unsubscribe();
        }
    }
}
