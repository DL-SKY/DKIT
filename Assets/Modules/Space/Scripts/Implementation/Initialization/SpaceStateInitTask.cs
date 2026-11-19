using System;
using Modules.Initializer.Scripts.Tasks;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.Space.Scripts.Implementation.State;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    public sealed class SpaceStateInitTask : TaskBase
    {
        private readonly SpaceStateManager _stateManager;
        private readonly SpaceDefinitionsManager _definitionsManager;

        public SpaceStateInitTask(int weight, SpaceStateManager stateManager, SpaceDefinitionsManager definitionsManager) : base(weight)
        {
            _stateManager = stateManager;
            _definitionsManager = definitionsManager;
        }

        public override void Run()
        {
            if (_stateManager == null || _definitionsManager == null)
            {
                Fail(SpaceInitErrorCodes.SAVE_FOLDER);
                return;
            }

            if (!SpaceQueueTime.CheckSamples())
            {
                UnityEngine.Debug.LogError("[Space] Queue time formula check failed.");
                Fail(SpaceInitErrorCodes.QUEUE_TIME);
                return;
            }

            try
            {
                _stateManager.Init(_definitionsManager);
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

            UnityEngine.Debug.Log($"[Space] Queue time check passed. Profile: {_stateManager.ProfileFilePath}");
            Complete();
        }
    }
}
