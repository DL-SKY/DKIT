using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;

namespace Modules.Initializer.Scripts.Tasks
{
    public class CloseViewTask : TaskBase
    {
        private readonly WindowsManager _windowsManager;
        private readonly int _handle;
        private readonly ViewModelBase _viewModel;

        public CloseViewTask(WindowsManager windowsManager, int handle, int weight) : base(weight)
        {
            _windowsManager = windowsManager;
            _handle = handle;
            _viewModel = null;
        }

        public CloseViewTask(WindowsManager windowsManager, ViewModelBase viewModel, int weight) : base(weight)
        {
            _windowsManager = windowsManager;
            _handle = -1;
            _viewModel = viewModel;
        }

        public override void Run()
        {
            _windowsManager.CloseView(GetHandle());
            Complete();
        }

        private int GetHandle()
        {
            if (_viewModel != null)
                return _viewModel.ViewHandle;
            else
                return _handle;
        }
    }
}
