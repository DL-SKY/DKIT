using UnityEngine.SceneManagement;

namespace Modules.Initializer.Scripts.Tasks
{
    public class LoadSceneTask : TaskBase
    {
        private string _sceneName;
        private LoadSceneMode _mode;

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
