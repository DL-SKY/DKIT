using System;
using Modules.Initializer.Scripts.Tasks;
using Modules.Space.Scripts.Implementation.State;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    public sealed class SpaceStateInitTask : TaskBase
    {
        private readonly SpaceStateManager _stateManager;

        public SpaceStateInitTask(int weight, SpaceStateManager stateManager) : base(weight)
        {
            _stateManager = stateManager;
        }

        public override void Run()
        {
            if (_stateManager == null)
            {
                Fail(SpaceInitErrorCodes.SAVE_FOLDER);
                return;
            }

            try
            {
                _stateManager.Init();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"[Space] Profile folder was not created: {exception.Message}");
                Fail(SpaceInitErrorCodes.SAVE_FOLDER);
                return;
            }

            if (!_stateManager.IsReady)
            {
                Fail(SpaceInitErrorCodes.SAVE_FOLDER);
                return;
            }

            UnityEngine.Debug.Log($"[Space] Profile directory: {_stateManager.ProfileDirectory}");
            Complete();
        }
    }
}
