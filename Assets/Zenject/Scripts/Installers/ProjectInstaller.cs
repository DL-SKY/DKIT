using Assets.Modules.Utils.Scripts.Components;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject.Scripts.Factories;

namespace Zenject.Scripts.Installers
{
    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        [SerializeField] private WindowsManager _windowsManagerPrefab;

        public override void InstallBindings()
        {
            //Container.Bind<GuiManager>().FromComponentInNewPrefab(_guiManager).AsSingle().NonLazy();
            //Container.Bind<EventSystem>().FromComponentInNewPrefab(_eventSystem).AsSingle().NonLazy();
            //Container.Bind<IViewAnimation>().To<ViewAnimationWithAnimator>().AsTransient();
            //Container.BindInterfacesAndSelfTo<SortingOrderManager>().AsSingle().NonLazy();

            //Core classes
            //Container.Bind<StateManager>().AsSingle().NonLazy();
            //Container.Bind<AdventuresManager>().AsSingle().NonLazy();
            Container.Bind<DefinitionsManager>().AsSingle().NonLazy();
            //...

            //Core prefabs
            Container.Bind<WindowsManager>().FromComponentInNewPrefab(_windowsManagerPrefab).AsSingle().NonLazy();
            Container.Bind<Updater>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            //...

            //Factories
            Container.Bind<ViewModelFactory>().AsSingle();
            //...
        }
    }
}
