using Modules.Initializer.Scripts.Tasks;
using Modules.Windows.Scripts.Implementation.Space;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Opens the empty sector window while the loader is still on top.
    /// </summary>
    public sealed class SpaceOpenMainViewTask : TaskBase
    {
        private readonly WindowsManager _windowsManager;
        private readonly ViewModelFactory _viewModelFactory;

        public SpaceOpenMainViewTask(int weight, WindowsManager windowsManager, ViewModelFactory viewModelFactory) : base(weight)
        {
            _windowsManager = windowsManager;
            _viewModelFactory = viewModelFactory;
        }

        public override void Run()
        {
            if (_windowsManager == null || _viewModelFactory == null)
            {
                Fail(SpaceInitErrorCodes.MAIN_VIEW);
                return;
            }

            if (Resources.Load<SpaceMainView>(SpaceMainView.Path) == null)
            {
                UnityEngine.Debug.LogError("[Space] Main view prefab was not found.");
                Fail(SpaceInitErrorCodes.MAIN_VIEW);
                return;
            }

            var viewModel = _viewModelFactory.Create<SpaceMainViewModel>();
            viewModel.Init();
            var view = _windowsManager.OpenView<SpaceMainView, SpaceMainViewModel>(SpaceMainView.Path, viewModel);
            if (view == null)
            {
                UnityEngine.Debug.LogError("[Space] Main view was not created.");
                Fail(SpaceInitErrorCodes.MAIN_VIEW);
                return;
            }

            Complete();
        }
    }
}
