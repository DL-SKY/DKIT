using Assets.Modules.Utils.Scripts.Components;
using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Tasks;
using Modules.Windows.Scripts.Implementation.Loading;
using Modules.Windows.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Initializer.Scripts.Implementation
{
    public class Initializer : MonoBehaviour
    {
        private WindowsManager _windowsManager;
        private Updater _updater;
        private ViewModelFactory _viewModelFactory;

        private void Start()
        {
            _windowsManager = ProjectContext.Instance.Container.Resolve<WindowsManager>();
            _updater = ProjectContext.Instance.Container.Resolve<Updater>();
            _viewModelFactory = ProjectContext.Instance.Container.Resolve<ViewModelFactory>();

            UnityEngine.Debug.LogError($"Initializer.Start() => _updater: {_updater != null}");
            UnityEngine.Debug.LogError($"                       _viewModelFactory: {_viewModelFactory != null}");

            var loaderViewModel = _viewModelFactory.Create<MainLoadViewModel>();
            //TODO: ...
            var tasks = new List<TaskBase>()
            {
                //new LogTask("01 Start", LogType.Error, 1),
                new PauseTask(_updater, 1, 1),
                //new LogTask("02", LogType.Error, 1),
                new PauseTask(_updater, 2, 2),
                //new LogTask("03", LogType.Error, 1),
                new PauseTask(_updater, 3, 3),
                //new LogTask("04", LogType.Error, 1),
                new PauseTask(_updater, 4, 4),
                //new LogTask("05", LogType.Error, 1),
                new PauseTask(_updater, 5, 5),
                //new LogTask("06 End", LogType.Error, 1),
                
                
                
                new PauseTask(_updater, 0.25f, 0),
                new CloseViewTask(_windowsManager, loaderViewModel, 0),
            };

            var tasker = new InitializeTasker(tasks);
            loaderViewModel.Init(tasker);
            var loaderView = _windowsManager.OpenView<MainLoadView, MainLoadViewModel>(MainLoadView.Path, loaderViewModel);

            tasker.OnProgressChange += (x, y) => UnityEngine.Debug.LogError($"{x}/{y}");
            tasker.Run(() => { }, (_) => { });
        }
    }
}
