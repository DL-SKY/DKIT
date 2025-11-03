using UnityEngine.SceneManagement;

namespace Modules.Initializer.Scripts.Tasks
{
    public class LoadSceneTask : TaskBase
    {
        private readonly string _sceneName;
        private readonly LoadSceneMode _mode;

        public LoadSceneTask(string sceneName, LoadSceneMode mode, int weight) : base(weight)
        {
            _sceneName = sceneName;
            _mode = mode;
        }

        public override void Run()
        {
            SceneManager.LoadScene(_sceneName, _mode);
            Complete();
        }
    }
}
