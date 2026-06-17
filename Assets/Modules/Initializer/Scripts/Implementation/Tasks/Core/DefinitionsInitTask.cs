using Modules.Initializer.Scripts.Tasks;
using Modules.Utils.Scripts.Components;
using System.Collections;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class DefinitionsInitTask : TaskBase
    {
        //[Inject] private readonly Definitions.Scripts.Implementation.Defs.DefinitionsManager _definitionsManager;
        [Inject] private readonly Definitions.Scripts.Implementation.Adventures.DefinitionsManager _definitionsManager;
        [Inject] private readonly CoroutineHolder _coroutineHolder;

        public DefinitionsInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            _coroutineHolder.StartCoroutine(InitAsync());
        }

        private IEnumerator InitAsync()
        {
            var asyncOperation = _definitionsManager.InitAsync();
            while (!asyncOperation.IsDone)
                yield return null;

            Complete();
        }
    }
}
