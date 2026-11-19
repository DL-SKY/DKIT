using Modules.Initializer.Scripts.Tasks;
using Modules.Space.Scripts.Implementation.Definitions;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    public sealed class SpaceDefinitionsInitTask : TaskBase
    {
        private readonly SpaceDefinitionsManager _definitionsManager;

        public SpaceDefinitionsInitTask(int weight, SpaceDefinitionsManager definitionsManager) : base(weight)
        {
            _definitionsManager = definitionsManager;
        }

        public override void Run()
        {
            if (_definitionsManager == null)
            {
                Fail(SpaceInitErrorCodes.DEFINITIONS);
                return;
            }

            _definitionsManager.Init();
            if (!_definitionsManager.IsReady)
            {
                UnityEngine.Debug.LogError("[Space] Definitions catalog is missing a hull, a module, an organ, or timed targets.");
                Fail(SpaceInitErrorCodes.DEFINITIONS);
                return;
            }

            UnityEngine.Debug.Log(
                $"[Space] Definitions ready. Hulls {_definitionsManager.Hulls.Count}, modules {_definitionsManager.Modules.Count}, organs {_definitionsManager.ControlOrgans.Count}, targets {_definitionsManager.TimedTargets.Count}.");
            Complete();
        }
    }
}
