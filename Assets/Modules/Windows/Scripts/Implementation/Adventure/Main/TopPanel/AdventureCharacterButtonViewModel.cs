using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Base;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    /// <summary>
    /// ViewModel for the active-character button in the adventure top panel (avatar icon).
    /// Lifetime is owned by <see cref="AdventureTopPanelViewModel"/>.
    /// </summary>
    public class AdventureCharacterButtonViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ICON = "ON_CHANGE_ICON";

        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Resources path for the active character avatar.
        /// Resolved by convention from <see cref="CharacterStateData.Avatar"/>.
        /// </summary>
        public string IconPath { get; private set; } = string.Empty;

        public bool IsDisposed => _isDisposed;

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            RefreshIcon(notify: false);
            Subscribe();
        }

        /// <summary>
        /// Player clicked the character button.
        /// </summary>
        public void OnClick()
        {
            if (_isDisposed)
                return;

            //TODO: ...
            UnityEngine.Debug.LogError($"AdventureCharacterButtonViewModel.OnClick()");
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            Unsubscribe();
            _isInitialized = false;
            IconPath = string.Empty;
        }

        private void Subscribe()
        {
            if (_stateLogic == null)
                return;

            // No fine-grained events for CurrentActiveCharacterId / Avatar —
            // refresh after any state action that may affect them.
            _stateLogic.StateChanged += OnStateChanged;
        }

        private void Unsubscribe()
        {
            if (_stateLogic == null)
                return;

            _stateLogic.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(StateChangeSource source)
        {
            if (_isDisposed)
                return;

            if (source != StateChangeSource.Characters &&
                source != StateChangeSource.CharactersAndInventory)
                return;

            RefreshIcon(notify: true);
        }

        private void RefreshIcon(bool notify)
        {
            string path = ResolveIconPath();
            if (IconPath == path)
                return;

            IconPath = path;

            if (notify)
                SendOnChange(ON_CHANGE_ICON);
        }

        private string ResolveIconPath()
        {
            CharacterStateData character = GetCurrentActiveCharacter();
            if (character == null || string.IsNullOrEmpty(character.Avatar))
                return _definitionsManager.VisualSettings.UnknownCharacterAvatar;

            return character.Avatar;
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
