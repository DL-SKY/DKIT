using Assets.Modules.Utils.Scripts.Components;

namespace Modules.Initializer.Scripts.Tasks
{
    public class PauseTask : TaskBase
    {
        private Updater _updater;
        private float _timer;

        public PauseTask(Updater updater, float duration, int weight) : base(weight)
        {
            _updater = updater;
            _timer = duration;
        }

        public override void Run()
        {
            _updater.OnUpdate += OnUpdateHandler;
        }

        private void OnUpdateHandler(float delta)
        {
            _timer -= delta;
            if (_timer <= 0.0f)
            {
                _updater.OnUpdate -= OnUpdateHandler;
                Complete();
            }
        }
    }
}
