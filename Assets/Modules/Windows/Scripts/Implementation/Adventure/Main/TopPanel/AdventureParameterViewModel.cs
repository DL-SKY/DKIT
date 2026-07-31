using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Base;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    /// <summary>
    /// ViewModel for a single adventure parameter slot in the top panel (icon + value).
    /// Lifetime is owned by <see cref="AdventureTopPanelViewModel"/>.
    /// </summary>
    public class AdventureParameterViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ICON = "ON_CHANGE_ICON";
        public const string ON_CHANGE_VALUE = "ON_CHANGE_VALUE";

        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Parameter key used for icon / value lookup (e.g. "STR", "Perception").
        /// </summary>
        public string ParameterName { get; private set; } = string.Empty;

        /// <summary>
        /// Resources path or remote URL for the parameter icon.
        /// Resolved from <c>VisualSettingsDef.ParameterIcons</c> by <see cref="ParameterName"/>.
        /// </summary>
        public string IconPath { get; private set; } = string.Empty;

        /// <summary>
        /// Display text for the parameter value (e.g. "+2", "0", "-1").
        /// </summary>
        public string Value { get; private set; } = string.Empty;

        public bool IsDisposed => _isDisposed;

        public void Init(string parameterName)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            ParameterName = parameterName ?? string.Empty;
            IconPath = ResolveIconPath(ParameterName);
            RefreshValue(notify: false);
            Subscribe();
        }

        /// <summary>
        /// Player clicked this parameter slot.
        /// </summary>
        public void OnClick()
        {
            if (_isDisposed)
                return;

            //TODO: ...
            UnityEngine.Debug.LogError($"AdventureParameterViewModel.OnClick()");
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            Unsubscribe();
            _isInitialized = false;
            ParameterName = string.Empty;
            IconPath = string.Empty;
            Value = string.Empty;
        }

        private void Subscribe()
        {
            if (_stateLogic == null)
                return;

            // No fine-grained events for CurrentActiveCharacterId / Parameters —
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

            RefreshValue(notify: true);
        }

        private void RefreshValue(bool notify)
        {
            string formatted = FormatParameterValue(ResolveParameterValue());
            if (Value == formatted)
                return;

            Value = formatted;

            if (notify)
                SendOnChange(ON_CHANGE_VALUE);
        }

        private int ResolveParameterValue()
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

        private RuleDef ResolveRuleDef()
        {
            string ruleId = _definitionsManager?.RuleSettings?.Rule;
            if (string.IsNullOrEmpty(ruleId) || _definitionsManager.Rules == null)
                return null;

            return _definitionsManager.Rules.TryGetValue(ruleId, out RuleDef ruleDef)
                ? ruleDef
                : null;
        }

        private string ResolveIconPath(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                return string.Empty;

            var parameterIcons = _definitionsManager?.VisualSettings?.ParameterIcons;
            if (parameterIcons == null)
                return string.Empty;

            return parameterIcons.TryGetValue(parameterName, out var iconPath) && iconPath != null
                ? iconPath
                : string.Empty;
        }

        private static string FormatParameterValue(int value)
        {
            if (value > 0)
                return "+" + value;

            return value.ToString();
        }
    }
}
