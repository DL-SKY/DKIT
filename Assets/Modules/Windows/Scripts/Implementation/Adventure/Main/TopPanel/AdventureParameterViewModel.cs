using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    public class AdventureParameterViewModel : AdventureParameterViewModelBase
    {
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;

        protected override void Subscribe()
        {
            _stateLogic.StateChanged += OnStateChanged;
        }

        protected override void Unsubscribe()
        {
            _stateLogic.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(StateChangeSource source)
        {
            if (IsDisposed)
                return;

            if (source != StateChangeSource.Characters &&
                source != StateChangeSource.CharactersAndInventory)
                return;

            RefreshValue(notify: true);
        }

        protected override int ResolveParameterValue()
        {
            if (string.IsNullOrEmpty(ParameterName))
                return 0;

            CharacterStateData character = GetCurrentActiveCharacter();
            if (character == null)
                return 0;

            var proxy = new CharacterParametersProxy(character, ResolveRuleDef(), _definitionsManager);
            return proxy.GetTotalValue(ParameterName);
        }

        private CharacterStateData GetCurrentActiveCharacter()
        {
            CharactersStateData charactersState = _stateManager?.State?.Characters;
            if (charactersState?.Characters == null)
                return null;

            int activeId = charactersState.CurrentActiveCharacterId;
            if (activeId <= 0)
                return null;

            return charactersState.Characters.TryGetValue(activeId, out CharacterStateData character)
                ? character
                : null;
        }
    }
}
