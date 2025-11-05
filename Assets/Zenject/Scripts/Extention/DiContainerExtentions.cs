namespace Zenject.Scripts.Extention
{
    public static class DiContainerExtentions
    {
        public static T ResolveFromRegistry<T>(this DiContainer container)
        {
            T result = default(T);

            SceneContextRegistry sceneContextRegistry = container.Resolve<SceneContextRegistry>();
            if (sceneContextRegistry != null)
            {
                result = sceneContextRegistry
                    .GetContainerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                    .Resolve<T>();
            }

            return result;
        }

        public static T TryResolveFromRegistry<T>(this DiContainer container) where T : class
        {
            if (!container.CheckSceneResolve())
                return null;

            T result = default(T);

            SceneContextRegistry sceneContextRegistry = container.Resolve<SceneContextRegistry>();
            if (sceneContextRegistry != null)
            {
                result = sceneContextRegistry
                    .GetContainerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                    .TryResolve<T>();
            }

            return result;
        }

        public static bool CheckSceneResolve(this DiContainer container)
        {
            SceneContextRegistry sceneContextRegistry = container.TryResolve<SceneContextRegistry>();
            if (sceneContextRegistry == null)
                return false;

            return sceneContextRegistry.TryGetContainerForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != null;
        }
    }
}