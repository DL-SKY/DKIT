using Assets.Modules.Utils.Scripts.Components;
using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Implementation.Tasks.Core;
using Modules.Initializer.Scripts.Tasks;
using Modules.Windows.Scripts.Implementation.Loading;
using Modules.Windows.Scripts.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.Scripts.Factories;
using Zenject.Scripts.Installers;

namespace Modules.Initializer.Scripts.Implementation
{
    public class Initializer : MonoBehaviour
    {
        private WindowsManager _windowsManager;
        private Updater _updater;
        private ViewModelFactory _viewModelFactory;

        private void Start()
        {
            var container = ProjectContext.Instance.Container;
            _windowsManager = container.Resolve<WindowsManager>();
            _updater = container.Resolve<Updater>();
            _viewModelFactory = container.Resolve<ViewModelFactory>();

            UnityEngine.Debug.LogError($"Initializer.Start() => _updater: {_updater != null}");
            UnityEngine.Debug.LogError($"                       _viewModelFactory: {_viewModelFactory != null}");

            var loaderViewModel = _viewModelFactory.Create<MainLoadViewModel>();

            var tasks = new List<TaskBase>()
            {
                //Core
                container.Instantiate<DefinitionsInitTask>(new object[] { 10 }),





                new PauseTask(_updater, 1f, 1),

                //new LoadSceneTask("SampleEcs", LoadSceneMode.Single, 1),
                new LoadSceneTask("Match3Scene", LoadSceneMode.Single, 1),

                new PauseTask(_updater, 1f, 1),





                //Show progress 100% pause
                new PauseTask(_updater, 0.25f, 0),
                new CloseViewTask(_windowsManager, loaderViewModel, 0),
            };

            DebugMethod01();

            var tasker = new InitializeTasker(tasks);
            loaderViewModel.Init(tasker);
            var loaderView = _windowsManager.OpenView<MainLoadView, MainLoadViewModel>(MainLoadView.Path, loaderViewModel);

            //TODO: subscribe and callbacks
            tasker.OnProgressChange += (x, y) => UnityEngine.Debug.LogError($"{x}/{y}");
            tasker.Run(OnCompletedCallback, OnFailedCallback);
        }

        private void OnCompletedCallback()
        {
            UnityEngine.Debug.LogError($"OnCompletedCallback() => ");
            DebugMethod01();
            //DebugMethod02();
        }

        private void OnFailedCallback(int error)
        {
            UnityEngine.Debug.LogError($"OnFailedCallback({error}) => ");
        }

        private void DebugMethod01()
        {
            var s = ProjectContext.Instance.Container.TryResolve<SceneContextRegistry>();
            var c = s.GetContainerForScene(SceneManager.GetActiveScene());
            var m3Prop = c?.TryResolve<M3Prop>();
            UnityEngine.Debug.LogError($"m3Prop: {m3Prop != null}");

            /*
             * DiContainer GetContainerForCurrentScene()
                {
                    return ProjectContext.Instance.Container.Resolve<SceneContextRegistry>()
                        .GetContainerForScene(gameObject.scene);
                }
             * */
        }

        private void DebugMethod02()
        {

            int[,] matrix = {
                { 1, 1, 1 },
                { 1, 0, 1 },
                { 1, 1, 1 }
            };

            // Сериализуем объект в JSON-строку
            string save = JsonConvert.SerializeObject(matrix, Formatting.Indented);

            UnityEngine.Debug.LogError($"matrix: {save}");

            //matrix:[
            //  [
            //    1,
            //    1,
            //    1
            //  ],
            //  [
            //    1,
            //    0,
            //    1
            //  ],
            //  [
            //    1,
            //    1,
            //    1
            //  ]
            //]
        }
    }
}
