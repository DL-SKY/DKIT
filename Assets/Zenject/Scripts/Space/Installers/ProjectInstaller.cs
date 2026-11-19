using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;
using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

namespace Zenject.Scripts.Space.Installers
{
    /// <summary>
    /// Project bindings for Space. Adventure services are not constructed.
    /// Loading the catalog and the profile stays in the startup tasks.
    /// </summary>
    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        [SerializeField] private WindowsManager _windowsManagerPrefab;

        public override void InstallBindings()
        {
            if (_windowsManagerPrefab == null)
                throw new System.InvalidOperationException("[Space] WindowsManager prefab is not assigned.");

            Container.Bind<Updater>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<CoroutineHolder>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<SpaceSessionHost>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.Bind<ViewModelFactory>().AsSingle();
            Container.Bind<EcsSystemFactory>().AsSingle();
            Container.Bind<WindowsManager>().FromComponentInNewPrefab(_windowsManagerPrefab).AsSingle().NonLazy();

            Container.Bind<SpaceDefinitionsManager>().AsSingle().NonLazy();
            Container.Bind<SpaceStateManager>().AsSingle().NonLazy();

            // Later, still not bound:
            // Screen input — taps that open the command menu.
            // IHintManager — when a Space window uses hints.
            // Space localization catalog — the startup task does not read Adventure settings.
            // Stash, blueprint charges, and hull certificates stay on the Space profile, not in EcsWorld.
            // SpaceSessionHost starts the flight world after the debug sector scene has loaded.
        }
    }
}
