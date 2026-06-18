using Modules.Initializer.Scripts.Core;
using Modules.Initializer.Scripts.Implementation.Tasks.Core;
using Modules.Initializer.Scripts.Tasks;
using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Implementation.Adventure.Main;
using Modules.Windows.Scripts.Implementation.Loading;
using Modules.Windows.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Initializer.Scripts.Implementation.Adventure
{
    /// <summary>
    /// TODO:
    /// Обязательно почистить код после реализации MVP
    /// </summary>
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
                //TODO: state task (load or create new state profile)
                container.Instantiate<DefinitionsInitTask>(new object[] { 10 }),
                container.Instantiate<AdventureStateInitTask>(new object[] { 10 }),
                container.Instantiate<LocalizationInitTask>(new object[] { 10 }),

                new PauseTask(_updater, 1f, 1),

                //new LoadSceneTask("SampleEcs", LoadSceneMode.Single, 1),
                //new LoadSceneTask(DEBUG_START_SCENE, LoadSceneMode.Single, _coroutineHolder, 1),

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
            tasker.OnProgressChange += (x, y) => UnityEngine.Debug.LogError($"...LOADING PROGRESS {x}/{y}...");
            tasker.Run(OnCompletedCallback, OnFailedCallback);
        }

        private void OnCompletedCallback()
        {
            UnityEngine.Debug.LogError($"OnCompletedCallback() => ");

            string DEBUG_START_SCENARIO = "Tavern";   //DEBUG
            var adventureMainViewModel = _viewModelFactory.Create<AdventureMainViewModel>();
            adventureMainViewModel.Init(DEBUG_START_SCENARIO);
            _windowsManager.OpenView<AdventureMainView, AdventureMainViewModel>(AdventureMainView.Path, adventureMainViewModel);
        }

        private void OnFailedCallback(int error)
        {
            UnityEngine.Debug.LogError($"OnFailedCallback({error}) => ");
        }
    }
}


    //public class TestViewModel : ViewModelBase
    //{
    //    [Inject] private readonly DiContainer _diContainer;

    //    private Match3RoundController _match3RoundController;

    //    public void Init()
    //    {
    //        _match3RoundController = _diContainer.TryResolveFromRegistry<Match3RoundController>();

    //        UnityEngine.Debug.LogError($"TestViewModel.Init() => _match3RoundController: {_match3RoundController != null}");
    //    }

    //    public override void Dispose()
    //    {
    //        //throw new System.NotImplementedException();
    //    }
    //}

