using Modules.Localization.Scripts.Core;
using TMPro;
using UnityEngine;

namespace Modules.Localization.Scripts.Components
{
    public abstract class LocalizationTextProxy : MonoBehaviour
    {
        protected bool _isInit;
        protected LocalizationManager _manager;
        protected TextMeshProUGUI _textComponent;

        private void Awake()
        {
            Init();
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

        private void OnChangeLanguageHandler(SystemLanguage _)
        { 
            //TODO: ...
            //...
        }

        protected abstract void Init(); //_localization = ProjectContext.Instance.Container.Resolve<Localization>();
    }
}
