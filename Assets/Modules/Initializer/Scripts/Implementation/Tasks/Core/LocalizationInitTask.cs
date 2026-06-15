using Modules.Initializer.Scripts.Tasks;
using Modules.Utils.Scripts.Components;
using System.Collections;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class LocalizationInitTask : TaskBase
    {
        //[Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly CoroutineHolder _coroutineHolder;

        public LocalizationInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            _coroutineHolder.StartCoroutine(InitAsync());
        }

        private IEnumerator InitAsync()
        {
            //var asyncOperation = _definitionsManager.Init();
            //while (!asyncOperation.isDone)
            //    yield return null;
            yield return null;

            Complete();
        }
    }
}
