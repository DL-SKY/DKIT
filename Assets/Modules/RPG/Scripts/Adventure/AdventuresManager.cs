using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using System;
using Zenject;

namespace Modules.RPG.Scripts.Adventure
{
    public class AdventuresManager : IDisposable
    {
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private string _adventureId;

        public void Init()
        {
            UnityEngine.Debug.LogError($"AdventuresManager.Init()");
            Subscribe();
        }

        public void Dispose()
        {
            UnityEngine.Debug.LogError($"AdventuresManager.Dispose()");
            Unsubscribe();
        }

        private void Subscribe()
        {
            _stateLogic.StateChanged += OnStateChangedHandler;
        }

        private void Unsubscribe()
        {
            _stateLogic.StateChanged -= OnStateChangedHandler;
        }

        private void OnStateChangedHandler(StateChangeSource source)
        {
            UnityEngine.Debug.Log($"[AdventuresManager] StateChanged: {source}, adventureId: {_adventureId}");
        }
    }
}
