using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Implementation.Tasks.Core;
using Modules.Initializer.Scripts.Tasks;
using Modules.Match3.Scripts.Implementation.Core;
using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Loading;
using Modules.Windows.Scripts.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.Scripts.Extention;
using Zenject.Scripts.Factories;

namespace Modules.Initializer.Scripts.Implementation
{
    public class Initializer : MonoBehaviour
    {
        private WindowsManager _windowsManager;
        private Updater _updater;
        private CoroutineHolder _coroutineHolder;
        private ViewModelFactory _viewModelFactory;

        private void Start()
        {
            var container = ProjectContext.Instance.Container;
            _windowsManager = container.Resolve<WindowsManager>();
            _updater = container.Resolve<Updater>();
            _coroutineHolder = container.Resolve<CoroutineHolder>();
            _viewModelFactory = container.Resolve<ViewModelFactory>();

            UnityEngine.Debug.LogError($"Initializer.Start() => _updater: {_updater != null}");
            UnityEngine.Debug.LogError($"                       _viewModelFactory: {_viewModelFactory != null}");

            var loaderViewModel = _viewModelFactory.Create<MainLoadViewModel>();

            var tasks = new List<TaskBase>()
            {
                //Filler
                //new PauseTask(_updater, 0.1f, 1),

                //Core
                container.Instantiate<DefinitionsInitTask>(new object[] { 10 }),
                container.Instantiate<LocalizationInitTask>(new object[] { 10 }),





                new PauseTask(_updater, 1f, 1),

                //new LoadSceneTask("SampleEcs", LoadSceneMode.Single, 1),
                new LoadSceneTask("Match3Scene", LoadSceneMode.Single, _coroutineHolder, 1),

                new PauseTask(_updater, 1f, 1),





                //Show progress 100% pause / filler
                new PauseTask(_updater, 0.25f, 0),
                new CloseViewTask(_windowsManager, loaderViewModel, 0),
            };

            //DebugMethod01();

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

            //DebugMethod01();
            //DebugMethod02();

            //var test = _viewModelFactory.Create<TestViewModel>();
            //test.Init();

            //TEST
            var container = ProjectContext.Instance.Container;
            var match3RoundController = container.TryResolveFromRegistry<Match3RoundController>();
            match3RoundController.Init("Example");
        }

        private void OnFailedCallback(int error)
        {
            UnityEngine.Debug.LogError($"OnFailedCallback({error}) => ");
        }

        private void DebugMethod01()
        {
            var s = ProjectContext.Instance.Container.TryResolve<SceneContextRegistry>();
            var c = s.GetContainerForScene(SceneManager.GetActiveScene());
            var match3RoundController = c?.TryResolve<Match3RoundController>();
            UnityEngine.Debug.LogError($"m3Prop: {match3RoundController != null}");

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





    public class TestViewModel : ViewModelBase
    {
        [Inject] private readonly DiContainer _diContainer;

        private Match3RoundController _match3RoundController;

        public void Init()
        {
            _match3RoundController = _diContainer.TryResolveFromRegistry<Match3RoundController>();

            UnityEngine.Debug.LogError($"TestViewModel.Init() => _match3RoundController: {_match3RoundController != null}");
        }

        public override void Dispose()
        {
            //throw new System.NotImplementedException();
        }
    }
}
