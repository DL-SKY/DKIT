using System.Collections.Generic;
using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Tasks;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;
using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Implementation.Loading;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Startup for the Space scene. Does not run Adventure or Match3 init tasks.
    /// </summary>
    public sealed class SpaceInitializer : MonoBehaviour
    {
        private const int TASK_WEIGHT = 10;

        private WindowsManager _windowsManager;
        private ViewModelFactory _viewModelFactory;
        private InitializeTasker _tasker;
        private SpaceSessionController _session;
        private SpaceDefinitionsManager _definitions;
        private SpaceStateManager _state;

        private void Start()
        {
            var container = ProjectContext.Instance.Container;
            _windowsManager = container.Resolve<WindowsManager>();
            _viewModelFactory = container.Resolve<ViewModelFactory>();

            _definitions = new SpaceDefinitionsManager();
            _state = new SpaceStateManager();
            var loaderViewModel = _viewModelFactory.Create<MainLoadViewModel>();

            var tasks = new List<TaskBase>
            {
                new SpaceDefinitionsInitTask(TASK_WEIGHT, _definitions),
                new SpaceStateInitTask(TASK_WEIGHT, _state, _definitions),
                new SpaceLocalizationInitTask(TASK_WEIGHT),
                new CloseViewTask(_windowsManager, loaderViewModel, 0),
            };

            _tasker = new InitializeTasker(tasks);
            loaderViewModel.Init(_tasker);
            _windowsManager.OpenView<MainLoadView, MainLoadViewModel>(MainLoadView.Path, loaderViewModel);
            _tasker.Run(OnCompleted, OnFailed);
        }

        private void OnCompleted()
        {
            UnityEngine.Debug.Log("[Space] Initialization complete. Sector is not started.");

            var container = ProjectContext.Instance.Container;
            _session = new SpaceSessionController(
                container.Resolve<Updater>(),
                container.Resolve<EcsSystemFactory>());
            _session.Start();
        }

        private void OnFailed(int errorCode)
        {
            UnityEngine.Debug.LogError($"[Space] Initialization failed with error {errorCode}.");
        }

        private void OnDestroy()
        {
            _session?.Dispose();
            _session = null;
            _tasker?.Dispose();
        }
    }
}
