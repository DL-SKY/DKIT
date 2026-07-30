using Modules.Windows.Scripts.Base;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.BottomPanel
{
    /// <summary>
    /// ViewModel for the adventure bottom panel.
    /// Owned by <see cref="AdventureMainViewModel"/>.
    /// </summary>
    public class AdventureBottomPanelViewModel : ViewModelBase
    {
        private bool _isInitialized;

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            Subscribe();
        }

        public override void Dispose()
        {
            Unsubscribe();
            _isInitialized = false;
        }

        private void Subscribe()
        {
        }

        private void Unsubscribe()
        {
        }
    }
}
