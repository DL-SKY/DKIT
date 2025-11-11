using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Initializer.Scripts.Tasks;
using Modules.Utils.Scripts.Components;
using System.Collections;
using System.Text;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class DefinitionsInitTask : TaskBase
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;
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



            var array = _definitionsManager.GameZones["Example"].Mask;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("2D Array:");
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(array[i, j] + "\t");
                }
                sb.AppendLine(); // Новая строка после каждой строки массива
            }
            UnityEngine.Debug.LogError(sb.ToString());



            Complete();
        }
    }
}
