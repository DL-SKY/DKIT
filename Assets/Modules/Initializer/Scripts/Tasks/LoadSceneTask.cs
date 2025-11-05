using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Initializer.Scripts.Tasks
{
    public class LoadSceneTask : TaskBase
    {
        private readonly string _sceneName;
        private readonly LoadSceneMode _mode;
        private readonly MonoBehaviour _coroutineHolder;

        public LoadSceneTask(string sceneName, LoadSceneMode mode, MonoBehaviour coroutineHolder, int weight) : base(weight)
        {
            _sceneName = sceneName;
            _mode = mode;
            _coroutineHolder = coroutineHolder;
        }

        public override void Run()
        {
            _coroutineHolder.StartCoroutine(LoadSceneAsync());
        }

        private IEnumerator LoadSceneAsync()
        {
            var asyncOperation = SceneManager.LoadSceneAsync(_sceneName, _mode);
            while (!asyncOperation.isDone)
                yield return null;

            Complete();
        }
    }
}
