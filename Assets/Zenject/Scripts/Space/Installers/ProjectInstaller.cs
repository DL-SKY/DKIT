using Modules.Space.Scripts.Implementation.Definitions;
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
            Container.Bind<ViewModelFactory>().AsSingle();
            Container.Bind<EcsSystemFactory>().AsSingle();
            Container.Bind<WindowsManager>().FromComponentInNewPrefab(_windowsManagerPrefab).AsSingle().NonLazy();

            Container.Bind<SpaceDefinitionsManager>().AsSingle().NonLazy();
            Container.Bind<SpaceStateManager>().AsSingle().NonLazy();

            // Later, still not bound:
            // CoroutineHolder — when a startup task needs a coroutine.
            // Screen input — camera drag and taps, with flight in step 4.
            // IHintManager — when a Space window uses hints.
            // Space localization catalog — the startup task does not read Adventure settings.
            // Stash, blueprint charges, and hull certificates stay on the Space profile, not in EcsWorld.
            // The flight world is created by SpaceInitializer after the startup tasks succeed.
        }
    }
}
