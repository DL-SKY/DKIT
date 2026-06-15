using Modules.Localization.Scripts.Core;
using TMPro;
using UnityEngine;

namespace Modules.Localization.Scripts.Components
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class LocalizationTextProxy : MonoBehaviour
    {
        protected bool _isInit;
        protected LocalizationManagerBase _manager;
        protected TextMeshProUGUI _textComponent;

        protected string _key;
        protected object[] _args;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();

            Init();
            TryApply();
        }

        private void OnEnable()
        {
            if (!_isInit)
                return;

            _manager.OnChangeLanguage += OnChangeLanguageHandler;
        }

        private void OnDisable()
        {
            if (!_isInit)
                return;

            _manager.OnChangeLanguage -= OnChangeLanguageHandler;
        }

        public void SetText(string key, params object[] args)
        {
            _key = key;
            _args = args;

            TryApply();
        }

        private void OnChangeLanguageHandler(SystemLanguage _)
        {
            TryApply();
        }

        private bool TryApply()
        {
            if (!_isInit)
                return false;

            if (string.IsNullOrEmpty(_key))
                return false;

            var localizedText = _manager.GetString(_key);
            if (string.IsNullOrEmpty(localizedText))
            {
                _textComponent.text = string.Empty;
                return false;
            }

            if (_args == null || _args.Length == 0)
            {
                _textComponent.text = localizedText;
                return true;
            }

            try
            {
                _textComponent.text = string.Format(localizedText, _args);
            }
            catch (System.FormatException ex)
            {
                UnityEngine.Debug.LogWarning($"[LocalizationTextProxy] Format failed for key '{_key}': {ex.Message}");
                _textComponent.text = localizedText;
                return false;
            }

            return true;
        }

        protected abstract void Init(); //_localization = ProjectContext.Instance.Container.Resolve<Localization>();
    }
}
