using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Restrictions.Scripts.Core;
using Modules.Utils.Scripts.Components;
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
            //Utils
            Container.Bind<Updater>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<CoroutineHolder>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<RestrictionsChecker>().AsSingle().NonLazy();
            //...

            //Container.Bind<GuiManager>().FromComponentInNewPrefab(_guiManager).AsSingle().NonLazy();
            //Container.Bind<EventSystem>().FromComponentInNewPrefab(_eventSystem).AsSingle().NonLazy();
            //Container.Bind<IViewAnimation>().To<ViewAnimationWithAnimator>().AsTransient();
            //Container.BindInterfacesAndSelfTo<SortingOrderManager>().AsSingle().NonLazy();

            //Core classes
            //Container.Bind<StateManager>().AsSingle().NonLazy();
            //Container.Bind<AdventuresManager>().AsSingle().NonLazy();
            Container.Bind<DefinitionsManager>().AsSingle().NonLazy();
            //LOCALIZATION!!!
            //...

            //Core prefabs
            Container.Bind<WindowsManager>().FromComponentInNewPrefab(_windowsManagerPrefab).AsSingle().NonLazy();            
            //...

            //Factories
            Container.Bind<ViewModelFactory>().AsSingle();
            Container.Bind<RestrictionFactory>().AsSingle();
            //...
        }
    }
}
