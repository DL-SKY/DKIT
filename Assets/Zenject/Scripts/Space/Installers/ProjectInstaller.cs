using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

namespace Zenject.Scripts.Space.Installers
{
    /// <summary>
    /// Project bindings for the Space scene. Does not construct Adventure services.
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
        }
    }
}
