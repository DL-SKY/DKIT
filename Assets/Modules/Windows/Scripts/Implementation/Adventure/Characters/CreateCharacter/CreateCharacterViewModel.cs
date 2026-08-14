using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CreateCharacter
{
    /// <summary>
    /// ViewModel for the character creation screen.
    /// Owns <see cref="CreateCharacterRequestData"/> accumulated during the flow.
    /// </summary>
    public class CreateCharacterViewModel : ViewModelBase
    {
        public const string ON_CHANGE_CHARACTER = "ON_CHANGE_CHARACTER";
        public const string ON_CHANGE_CAN_CREATE = "ON_CHANGE_CAN_CREATE";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Accumulator for the character being created. Filled by UI later; submitted on Create.
        /// </summary>
        private CreateCharacterRequestData _request;

        /// <summary>
        /// Write API for the draft. Pass into sub-window VMs; do not mutate <c>_request</c> directly.
        /// </summary>
        private CreateCharacterRequestApplicator _applicator;

        public string Name { get; private set; }
        public string Avatar { get; private set; }
        public string HitPointsTitle { get; private set; }
        public int HitPoints { get; private set; }
        public string LevelTitle { get; private set; }
        public int Level { get; private set; }
        public string AncestryTitle { get; private set; }
        public string Ancestry { get; private set; }                // checkpoint
        public string ClassTitle { get; private set; }
        public string Class { get; private set; }                   // checkpoint
        public string BackgroundTitle { get; private set; }
        public string Background { get; private set; }              // checkpoint
        public string AbilityScoresTitle { get; private set; }
        public string AbilityBoostPointsTitle { get; private set; }
        public int AbilityBoostPoints { get; private set; }         // checkpoint
        public string SkillsTitle { get; private set; }
        public string SkillsBoostPointsTitle { get; private set; }
        public int SkillsBoostPoints { get; private set; }          // checkpoint
        public bool CanCreate { get; private set; }

        public void Init(bool addToActiveParty)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _isDisposed = false;

            _request = new CreateCharacterRequestData
            {
                AddToActiveParty = addToActiveParty,
                CharacterData = new CharacterRequestData(),
            };

            _applicator = new CreateCharacterRequestApplicator(_request, _definitionsManager);
            _request.OnUpdate += OnRequestUpdated;

            _applicator.RebuildDerived(notify: false);
            UpdateCharacterRequestData();
            RefreshCanCreate();
        }

        private void OnRequestUpdated()
        {
            if (_isDisposed)
                return;

            UpdateCharacterRequestData();
            RefreshCanCreate();
        }

        private void UpdateCharacterRequestData()
        {
            //...

            SendOnChange(ON_CHANGE_CHARACTER);
        }

        private void RefreshCanCreate()
        {
            bool canCreate = _request != null
                && !string.IsNullOrWhiteSpace(_request.Name)
                && !string.IsNullOrWhiteSpace(_request.Ancestry)
                && !string.IsNullOrWhiteSpace(_request.Class);

            SetCanCreate(canCreate);
        }

        private void SetCanCreate(bool value)
        {
            if (CanCreate == value)
                return;

            CanCreate = value;
            SendOnChange(ON_CHANGE_CAN_CREATE);
        }

        /// <summary>
        /// Back: close this window and return to the adventure screen underneath.
        /// </summary>
        public void OnClose()
        {
            if (_isDisposed)
                return;

            Close();
        }

        /// <summary>
        /// Active Create: persist character + party membership, set active character id, then close.
        /// </summary>
        public void OnCreateActive()
        {
            if (_isDisposed)
                return;

            if (!CanCreate)
                return;

            if (_request == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CreateCharacterViewModel)}] {nameof(_request)} is null. Call {nameof(Init)} first.");

                Close();

                return;
            }

            int newCharacterId = _stateManager.State?.Characters?.NextCharacterId ?? 0;

            StateActionValidationResult createResult =
                _stateLogic.ProcessAction(new CreateCharacterStateAction(_request));
            if (!createResult.IsValid)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CreateCharacterViewModel)}] Create character failed " +
                    $"(code {createResult.ErrorCode}): {createResult.ErrorMessage}");

                Close();

                return;
            }

            StateActionValidationResult setActiveResult =
                _stateLogic.ProcessAction(new SetCurrentActiveCharacterIdStateAction(newCharacterId));
            if (!setActiveResult.IsValid)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CreateCharacterViewModel)}] Set active character id={newCharacterId} failed " +
                    $"(code {setActiveResult.ErrorCode}): {setActiveResult.ErrorMessage}");

                Close();

                return;
            }

            Close();
        }

        /// <summary>
        /// Disabled Create: character is not ready to create yet.
        /// </summary>
        public void OnCreateDisable()
        {
            if (_isDisposed)
                return;

            // Intentional no-op for now (future: hint / validation feedback).

            // TODO: ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜-˜˜ ˜˜˜˜-˜˜ ˜˜˜˜-˜˜...
            //...
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }


        public void OnEditName()
        {

        }

        public void OnEditAvatar()
        {

        }

        public void OnEditAncestry()
        {

        }

        public void OnEditClass()
        {

        }

        public void OnEditBackground()
        {

        }

        public void OnEditAbilities()
        {

        }

        public void OnEditSkills()
        {

        }


        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isInitialized = false;

            if (_request != null)
                _request.OnUpdate -= OnRequestUpdated;

            _applicator?.Dispose();
            _applicator = null;
            _request = null;
            CanCreate = false;
        }
    }
}
