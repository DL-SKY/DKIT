using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Initializer.Scripts.Tasks;
using Modules.RPG.Scripts.Adventure;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class AdventuresManagerInitTask : TaskBase
    {
        [Inject] private readonly AdventuresManager _adventuresManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public AdventuresManagerInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            _adventuresManager.Init();
            Complete();
        }
    }
}
