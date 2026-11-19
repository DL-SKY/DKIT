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
                Fail(SpaceInitErrorCodes.DEFINITIONS);
                return;
            }

            UnityEngine.Debug.Log("[Space] Definitions catalog is ready. Records are added in the data step.");
            Complete();
        }
    }
}
