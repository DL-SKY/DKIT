using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Initializer.Scripts.Tasks;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class DefinitionsInitTask : TaskBase
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public DefinitionsInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            _definitionsManager.Init();
            Complete();
        }
    }
}
