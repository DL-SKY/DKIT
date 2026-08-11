using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Localization.Scripts.Core;
using Modules.Localization.Scripts.Implementation;
using Modules.Restrictions.Scripts.Core;
using Modules.State.Scripts.Implementation.Match3;
using Modules.State.Scripts.Implementation.Match3.Logic;
using Modules.Utils.Scripts.Components;
using Modules.Utils.Scripts.Input;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Services;
using UnityEngine;
using Zenject;
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
            Container.BindInterfacesAndSelfTo<ScreenInputService>().AsSingle().NonLazy();
            Container.Bind<RestrictionsChecker>().AsSingle().NonLazy();
            Container.Bind<IImageCache>().To<CachedPathImageService>().AsSingle().NonLazy();
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
            Container.Bind<IHintManager>().FromMethod(ResolveHintManager).AsSingle();
            //...

            //Factories
            Container.Bind<ViewModelFactory>().AsSingle();
            Container.Bind<RestrictionFactory>().AsSingle();
            //...

            //Debug
            Container.BindInterfacesAndSelfTo<Modules.Debug.Scripts.Logger.Logger>().AsSingle().NonLazy();  //IDisposable
        }

        private static IHintManager ResolveHintManager(InjectContext context)
        {
            WindowsManager windowsManager = context.Container.Resolve<WindowsManager>();
            IHintManager hintManager = windowsManager.GetComponent<IHintManager>();
            if (hintManager == null)
            {
                throw new System.InvalidOperationException(
                    "IHintManager component is missing on the WindowsManager prefab. " +
                    "Run Tools/Cursor/Wire HintManager On WindowsManager (or Build Hint Prefabs).");
            }

            return hintManager;
        }
    }
}

// Example
//Container.Bind<GuiManager>().FromComponentInNewPrefab(_guiManager).AsSingle().NonLazy();
//Container.Bind<EventSystem>().FromComponentInNewPrefab(_eventSystem).AsSingle().NonLazy();
//Container.Bind<IViewAnimation>().To<ViewAnimationWithAnimator>().AsTransient();
//Container.BindInterfacesAndSelfTo<SortingOrderManager>().AsSingle().NonLazy();
