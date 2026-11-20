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
                UnityEngine.Debug.LogError("[Space] Profile has no valid active hull.");
                Fail(SpaceInitErrorCodes.HULL);
                return;
            }

            var profile = _stateManager.Profile;
            int fittedCount = CountInstalledModules(profile);
            UnityEngine.Debug.Log(
                $"[Space] Queue time check passed. Hull {profile.ActiveHullId}, fitted {fittedCount}. Profile: {_stateManager.ProfileFilePath}");
            Complete();
        }

        private static int CountInstalledModules(SpaceProfile profile)
        {
            var count = 0;
            if (profile?.Ships == null)
                return count;

            for (var i = 0; i < profile.Ships.Count; i++)
            {
                var ship = profile.Ships[i];
                if (ship == null || ship.HullId != profile.ActiveHullId || ship.Slots == null)
                    continue;

                for (var slotIndex = 0; slotIndex < ship.Slots.Count; slotIndex++)
                {
                    var slot = ship.Slots[slotIndex];
                    if (slot != null && !string.IsNullOrEmpty(slot.ModuleId))
                        count++;
                }
            }

            return count;
        }
    }
}
