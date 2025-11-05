using Modules.Definitions.Scripts.Core;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Utils.Scripts.Components;
using Modules.Utils.Scripts.UnityImplementation;
using System;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Modules.Definitions.Scripts.Implementation.Defs
{
    public class DefinitionsManager
    {
        [Inject] private readonly CoroutineHolder _coroutineHolder;

        public Dictionary<string, GameZoneDef> GameZones;        

        private readonly Loader _loader;
        private readonly SimpleAsyncOperation _asyncOperation;


        public DefinitionsManager()
        {
            _loader = new Loader();
            _asyncOperation = new SimpleAsyncOperation();
        }


        public SimpleAsyncOperation InitAsync()
        {
            _coroutineHolder.StartCoroutine(LoadAll());
            return _asyncOperation;
        }

        private IEnumerator LoadAll()
        {
            var loadMethods = new List<Action>{
                LoadGameZones,
            };            

            foreach (var method in loadMethods)
            {
                method.Invoke();
                yield return null;
            }

            _asyncOperation.SetCompleted();
        }

        private void LoadGameZones()
        {
            GameZones = _loader.LoadCollection<GameZoneDef>("Definitions/GameZones");
        }
    }
}
