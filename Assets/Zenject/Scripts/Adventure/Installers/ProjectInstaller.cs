using Modules.RPG.Scripts.Adventure;
using Modules.Localization.Scripts.Core;
using Modules.Localization.Scripts.Implementation;
using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Factories;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject.Scripts.Factories;

namespace Zenject.Scripts.Adventure.Installers
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

            //Core classes
            Container.Bind<Modules.Definitions.Scripts.Implementation.Adventures.DefinitionsManager>().AsSingle().NonLazy();
            Container.Bind<AdventureStateManager>().AsSingle().NonLazy();
            Container.Bind<AdventureStateLogic>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AdventuresManager>().AsSingle().NonLazy();
            Container.Bind<LocalizationManager>().AsSingle().NonLazy();
            Container.Bind<LocalizationManagerBase>().To<LocalizationManager>().FromResolve();
            //...

            //Core prefabs
            Container.Bind<WindowsManager>().FromComponentInNewPrefab(_windowsManagerPrefab).AsSingle().NonLazy();
            //...

            //Factories
            Container.Bind<IAdventureStateDataFactory>().To<AdventureStateDataFactory>().AsTransient();
            Container.Bind<ViewModelFactory>().AsSingle();
            Container.Bind<RestrictionFactory>().AsSingle();
            //...

            //Debug
            Container.BindInterfacesAndSelfTo<Modules.Debug.Scripts.Logger.Logger>().AsSingle().NonLazy();  //IDisposable
        }
    }
}

// Example
//Container.Bind<GuiManager>().FromComponentInNewPrefab(_guiManager).AsSingle().NonLazy();
//Container.Bind<EventSystem>().FromComponentInNewPrefab(_eventSystem).AsSingle().NonLazy();
//Container.Bind<IViewAnimation>().To<ViewAnimationWithAnimator>().AsTransient();
//Container.BindInterfacesAndSelfTo<SortingOrderManager>().AsSingle().NonLazy();
