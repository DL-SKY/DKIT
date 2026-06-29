using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Localization.Scripts.Core;
using Modules.Localization.Scripts.Implementation;
using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Match3;
using Modules.State.Scripts.Implementation.Match3.Logic;
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

            //Core classes
            Container.Bind<DefinitionsManager>().AsSingle().NonLazy();
            Container.Bind<Match3StateManager>().AsSingle().NonLazy();
            Container.Bind<Match3StateLogic>().AsSingle().NonLazy();
            Container.Bind<LocalizationManager>().AsSingle().NonLazy();
            Container.Bind<LocalizationManagerBase>().To<LocalizationManager>().FromResolve();
            //...

            //Core prefabs
            Container.Bind<WindowsManager>().FromComponentInNewPrefab(_windowsManagerPrefab).AsSingle().NonLazy();
            //...

            //Factories
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
