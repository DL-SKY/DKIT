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

            _textComponent.text = string.Format(_manager.GetString(_key), _args);

            return true;
        }

        protected abstract void Init(); //_localization = ProjectContext.Instance.Container.Resolve<Localization>();
    }
}
