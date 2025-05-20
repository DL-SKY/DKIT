using System;
using UnityEngine;

namespace Modules.Localization.Scripts.Core
{
    public abstract class LocalizationManagerBase
    {
        public Action<SystemLanguage> OnChangeLanguage;

        public SystemLanguage Language => _language;
        protected SystemLanguage _language;

        public int Version => _data?.Version ?? -1;

        public string Description => _data?.Description ?? string.Empty;

        protected LocalizationData _data;

        public void Init()
        {
            TrySetLanguage(GetCurrentLanguage());
        }

        public bool TrySetLanguage(SystemLanguage newLanguage)
        {
            if (CheckAvailableLanguage(newLanguage))
            {
                _language = newLanguage;
                _data = LoadLanguage(Language);

                OnChangeLanguage?.Invoke(Language);
            }

            UnityEngine.Debug.LogWarning($"[LocalizationManager] TrySetLanguage({newLanguage}) -> not supported language: {newLanguage}!");
            return false;
        }

        public string GetString(string key)
        {
            if (_data == null)
            {
                UnityEngine.Debug.LogWarning($"[LocalizationManager] GetString({key}) -> data is NULL!");
                return string.Empty;
            }

            if (_data.Locals.TryGetValue(key, out var value))
            {
                return value;
            }

            UnityEngine.Debug.LogWarning($"[LocalizationManager] GetString({key}) -> not contains key: {key}!");
            return string.Empty;
        }

        protected abstract SystemLanguage GetCurrentLanguage();

        protected abstract bool CheckAvailableLanguage(SystemLanguage language);

        protected abstract LocalizationData LoadLanguage(SystemLanguage language);
    }
}
