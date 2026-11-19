using Modules.Utils.Scripts.Components;
using Modules.Windows.Scripts.Managers;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Bindings for the Space scene. Adventure services are not constructed.
    /// </summary>
    public static class SpaceProjectInstaller
    {
        public static bool IsSpaceScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
                return false;

            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i].GetComponentInChildren<SpaceInitializer>(true) != null)
                    return true;
            }

            return false;
        }

        public static void Install(DiContainer container, WindowsManager windowsManagerPrefab)
        {
            if (windowsManagerPrefab == null)
                throw new System.InvalidOperationException("[Space] WindowsManager prefab is not assigned.");

            container.Bind<Updater>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            container.Bind<ViewModelFactory>().AsSingle();
            container.Bind<EcsSystemFactory>().AsSingle();
            container.Bind<WindowsManager>().FromComponentInNewPrefab(windowsManagerPrefab).AsSingle().NonLazy();
        }
    }
}
