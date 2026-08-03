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
        public const string ON_CHANGE_CAN_CREATE = "ON_CHANGE_CAN_CREATE";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Accumulator for the character being created. Filled by UI later; submitted on Create.
        /// </summary>
        private CreateCharacterRequestData _request { get; set; }

        /// <summary>
        /// When true, the active Create button is shown; otherwise the disabled Create button.
        /// </summary>
        public bool CanCreate { get; private set; }

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _isDisposed = false;

            _request = new CreateCharacterRequestData
            {
                AddToActiveParty = true,
                CharacterData = new CharacterRequestData(),
            };

            SetCanCreate(false);
        }

        /// <summary>
        /// Updates Create button visibility (active vs disabled).
        /// </summary>
        public void SetCanCreate(bool value)
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

            // TODO: добавить уведомление о недоступности кнопки из-за того-то того-то...
            //...
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isInitialized = false;
            _request = null;
            CanCreate = false;
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }
    }
}
