using System.Collections;
using Modules.Initializer.Scripts.Tasks;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Loads the scene named by a sector definition. The same task loads every space sector.
    /// </summary>
    public sealed class SpaceLoadSectorTask : TaskBase
    {
        private readonly string _sectorId;
        private readonly SpaceDefinitionsManager _definitions;
        private readonly MonoBehaviour _coroutineHolder;

        public SpaceLoadSectorTask(
            int weight,
            string sectorId,
            SpaceDefinitionsManager definitions,
            MonoBehaviour coroutineHolder) : base(weight)
        {
            _sectorId = sectorId;
            _definitions = definitions;
            _coroutineHolder = coroutineHolder;
        }

        public override void Run()
        {
            if (_coroutineHolder == null || !SpaceSectorLoad.TryGet(_definitions, _sectorId, out var sector))
            {
                UnityEngine.Debug.LogError($"[Space] Sector '{_sectorId}' is missing or has no scene and spawn point.");
                Fail(SpaceInitErrorCodes.SECTOR);
                return;
            }

            UnityEngine.Debug.Log($"[Space] Sector '{_sectorId}' loads scene '{sector.SceneName}'. Spawn points {sector.SpawnPoints.Count}.");
            _coroutineHolder.StartCoroutine(LoadScene(sector.SceneName));
        }

        private IEnumerator LoadScene(string sceneName)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (asyncOperation == null)
            {
                UnityEngine.Debug.LogError($"[Space] Scene '{sceneName}' is not in the build settings.");
                Fail(SpaceInitErrorCodes.SECTOR);
                yield break;
            }

            while (!asyncOperation.isDone)
                yield return null;

            Complete();
        }
    }
}
