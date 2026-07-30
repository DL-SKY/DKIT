using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.RPG.Scripts.Adventure.Choice;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Base;
using System;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice
{
    /// <summary>
    /// Base ViewModel for a single adventure choice panel.
    /// Created via Zenject; call <see cref="Init"/> before use.
    /// </summary>
    public abstract class AdventureChoiceViewModelBase : ViewModelBase
    {
        private const string DEFAULT_TYPE_MAIN_ICON = "Adventures/Icons/W_047";
        private const string DICE_CHECK_TYPE_MAIN_ICON = "Adventures/Icons/dice_20";
        private const string TEXT_PARAMS_SUFFIX = ".TextParams";

        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly AdventureStateManager _stateManager;

        private bool _isDisposed;

        public ChoiceData Data { get; private set; }

        public ChoiceType ChoiceType => Data.Type;

        public string Text { get; private set; }
        public string Description { get; private set; }
        public string DescriptionParam { get; private set; }
        public string MainIcon { get; private set; }
        public bool EnabledDescription { get; private set; }
        public string DescriptionIcon { get; private set; }

        public bool IsDisposed => _isDisposed;

        public virtual void Init(ChoiceData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));

            MainIcon = GetMainIcon();
            Text = GetText();
            Description = GetDescription();
            DescriptionIcon = GetDescriptionIcon();

            EnabledDescription = !string.IsNullOrEmpty(Description);
        }

        /// <summary>
        /// Player selected this choice. Subclasses execute actions / open dice check UI.
        /// </summary>
        public abstract void Select();

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            DisposeImplementation();
        }

        /// <summary>
        /// Override for subclass cleanup. Called once from <see cref="Dispose"/>.
        /// </summary>
        protected virtual void DisposeImplementation()
        {
        }

        private string GetMainIcon()
        {
            string mainIcon = Data.VisualOptions?.MainIcon;
            if (!string.IsNullOrEmpty(mainIcon))
                return mainIcon;

            return Data.Type == ChoiceType.DiceCheck
                ? DICE_CHECK_TYPE_MAIN_ICON
                : DEFAULT_TYPE_MAIN_ICON;
        }

        private string GetText()
        {
            return Data.Text ?? string.Empty;
        }

        private string GetDescription()
        {
            string parameterOverride = Data.VisualOptions?.ParameterOverrideDescription;
            if (!string.IsNullOrEmpty(parameterOverride))
            {
                DescriptionParam = FormatParameterValue(ResolveParameterValue(parameterOverride));
                return ResolveParameterLocalization(parameterOverride + TEXT_PARAMS_SUFFIX);
            }

            DescriptionParam = string.Empty;
            return Data.Description ?? string.Empty;
        }

        private string GetDescriptionIcon()
        {
            string parameterOverride = Data.VisualOptions?.ParameterOverrideDescription;
            if (!string.IsNullOrEmpty(parameterOverride))
                return ResolveParameterIcon(parameterOverride);

            return Data.VisualOptions?.DescriptionIcon ?? string.Empty;
        }

        private int ResolveParameterValue(string parameterName)
        {
            CharacterStateData character = GetCurrentActiveCharacter();
            if (character == null)
                return 0;

            var proxy = new CharacterParametersProxy(character, ResolveRuleDef(), _definitionsManager);
            return proxy.GetTotalValue(parameterName);
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

        private string ResolveParameterLocalization(string localizationKey)
        {
            var parameterLocalizations = _definitionsManager?.VisualSettings?.ParameterLocalizations;
            if (parameterLocalizations == null)
                return string.Empty;

            return parameterLocalizations.TryGetValue(localizationKey, out var localization)
                   && localization != null
                ? localization
                : string.Empty;
        }

        private string ResolveParameterIcon(string parameterName)
        {
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
