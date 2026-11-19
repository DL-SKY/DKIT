using System.Collections.Generic;
using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Tasks;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;
using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Implementation.Space;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Startup for the Space scene. Does not run Adventure or Match3 init tasks.
    /// </summary>
    public sealed class SpaceInitializer : MonoBehaviour
    {
        public const string DEBUG_SECTOR_SCENE = "SpaceDebug";

        private const int TASK_WEIGHT = 10;
        private const float FULL_BAR_PAUSE_SECONDS = 0.25f;

        private WindowsManager _windowsManager;
        private ViewModelFactory _viewModelFactory;
        private InitializeTasker _tasker;
        private bool _startupFinished;

        private void Start()
        {
            var container = ProjectContext.Instance.Container;
            _windowsManager = container.Resolve<WindowsManager>();
            _viewModelFactory = container.Resolve<ViewModelFactory>();
            var definitions = container.Resolve<SpaceDefinitionsManager>();
            var state = container.Resolve<SpaceStateManager>();
            var coroutineHolder = container.Resolve<CoroutineHolder>();
            var updater = container.Resolve<Updater>();
            var loaderViewModel = _viewModelFactory.Create<SpaceLoadViewModel>();

            var tasks = new List<TaskBase>
            {
                new SpaceDefinitionsInitTask(TASK_WEIGHT, definitions),
                new SpaceStateInitTask(TASK_WEIGHT, state, definitions),
                new SpaceLocalizationInitTask(TASK_WEIGHT),
                new LoadSceneTask(DEBUG_SECTOR_SCENE, LoadSceneMode.Single, coroutineHolder, TASK_WEIGHT),
                new PauseTask(updater, FULL_BAR_PAUSE_SECONDS, 0),
                new SpaceOpenMainViewTask(0, _windowsManager, _viewModelFactory),
                new CloseViewTask(_windowsManager, loaderViewModel, 0),
            };

            _tasker = new InitializeTasker(tasks);
            loaderViewModel.Init(_tasker);
            _windowsManager.OpenView<SpaceLoadView, SpaceLoadViewModel>(SpaceLoadView.Path, loaderViewModel);
            _tasker.Run(OnCompleted, OnFailed);
        }

        private void OnCompleted()
        {
            _startupFinished = true;
            _tasker?.Dispose();
            _tasker = null;

            UnityEngine.Debug.Log("[Space] Initialization complete. Debug sector loaded.");

            var container = ProjectContext.Instance.Container;
            var host = container.Resolve<SpaceSessionHost>();
            host.StartSession(
                container.Resolve<Updater>(),
                container.Resolve<EcsSystemFactory>(),
                container.Resolve<SpaceDefinitionsManager>(),
                container.Resolve<SpaceStateManager>());
        }

        private void OnFailed(int errorCode)
        {
            _startupFinished = true;
            _tasker?.Dispose();
            _tasker = null;
            UnityEngine.Debug.LogError($"[Space] Initialization failed with error {errorCode}.");
        }

        private void OnDestroy()
        {
            if (!_startupFinished)
                return;

            _tasker?.Dispose();
            _tasker = null;
        }
    }
}
