using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.Windows.Scripts.Base;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    public abstract class AdventureParameterViewModelBase : ViewModelBase
    {
        public const string ON_CHANGE_ICON = "ON_CHANGE_ICON";
        public const string ON_CHANGE_VALUE = "ON_CHANGE_VALUE";

        [Inject] protected readonly DefinitionsManager _definitionsManager;

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
        public string Value { get; protected set; } = string.Empty;

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
            UnityEngine.Debug.LogError($"{GetType().Name}.OnClick() :: {ParameterName}");
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

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract int ResolveParameterValue();

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

        protected string FormatParameterValue(int value)
        {
            if (value > 0)
                return "+" + value;

            return value.ToString();
        }

        protected RuleDef ResolveRuleDef()
        {
            string ruleId = _definitionsManager?.RuleSettings?.Rule;
            if (string.IsNullOrEmpty(ruleId) || _definitionsManager.Rules == null)
                return null;

            return _definitionsManager.Rules.TryGetValue(ruleId, out RuleDef ruleDef)
                ? ruleDef
                : null;
        }

        protected void RefreshValue(bool notify)
        {
            string formatted = FormatParameterValue(ResolveParameterValue());
            if (Value == formatted)
                return;

            Value = formatted;

            if (notify)
                SendOnChange(ON_CHANGE_VALUE);
        }
    }
}
