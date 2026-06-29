using Modules.Definitions.Scripts.Implementation.Defs.Single;
using Modules.Localization.Scripts.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Modules.Localization.Scripts.Implementation
{
    public class LocalizationManager : LocalizationManagerBase
    {
        private const string LANGUAGE_RESOURCE_PATTERN = "Langs/{0}/Localization";

        [InjectOptional] private readonly Modules.Definitions.Scripts.Implementation.Adventures.DefinitionsManager _adventureDefinitionsManager;

        private LocalizationSettingsDef _settings;

        public SystemLanguage ResolveSystemLanguageOrDefault()
        {
            EnsureSettingsLoaded();

            var systemLanguage = Application.systemLanguage;
            if (systemLanguage == SystemLanguage.Unknown)
                return _settings.DefaultLanguage;

            if (CheckAvailableLanguage(systemLanguage))
                return systemLanguage;

            return _settings.DefaultLanguage;
        }

        public SystemLanguage GetDefaultLanguage()
        {
            EnsureSettingsLoaded();
            return _settings.DefaultLanguage;
        }

        protected override SystemLanguage GetCurrentLanguage()
        {
            return ResolveSystemLanguageOrDefault();
        }

        protected override bool CheckAvailableLanguage(SystemLanguage language)
        {
            EnsureSettingsLoaded();

            if (_settings.LanguageFolders == null)
                return false;

            if (!_settings.LanguageFolders.TryGetValue(language, out var folder))
                return false;

            if (string.IsNullOrWhiteSpace(folder))
                return false;

            var resourcePath = string.Format(LANGUAGE_RESOURCE_PATTERN, folder);
            var asset = Resources.Load<TextAsset>(resourcePath);
            return asset != null;
        }

        protected override LocalizationData LoadLanguage(SystemLanguage language)
        {
            EnsureSettingsLoaded();

            if (_settings.LanguageFolders == null || !_settings.LanguageFolders.TryGetValue(language, out var folder))
            {
                UnityEngine.Debug.LogWarning($"[LocalizationManager] LoadLanguage({language}) -> folder mapping was not found.");
                return _data;
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                UnityEngine.Debug.LogWarning($"[LocalizationManager] LoadLanguage({language}) -> folder is empty.");
                return _data;
            }

            var resourcePath = string.Format(LANGUAGE_RESOURCE_PATTERN, folder);
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                UnityEngine.Debug.LogWarning($"[LocalizationManager] LoadLanguage({language}) -> TextAsset was not found by path '{resourcePath}'.");
                return _data;
            }

            LocalizationData loadedData;
            try
            {
                loadedData = JsonConvert.DeserializeObject<LocalizationData>(asset.text);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[LocalizationManager] LoadLanguage({language}) -> deserialization failed: {ex.Message}");
                return _data;
            }

            if (loadedData == null)
            {
                UnityEngine.Debug.LogWarning($"[LocalizationManager] LoadLanguage({language}) -> deserialized data is null.");
                return _data;
            }

            if (_data == null)
            {
                _data = new LocalizationData();                
            }

            _data.Version = loadedData.Version;
            _data.Description = loadedData.Description;
            _data.Locals = new Dictionary<string, string>();

            if (loadedData.Locals == null)
            {
                return _data;
            }

            foreach (var pair in loadedData.Locals)
            {
                if (!_data.Locals.TryAdd(pair.Key, pair.Value))
                {
                    UnityEngine.Debug.LogWarning($"[LocalizationManager] LoadLanguage({language}) -> duplicate key skipped: {pair.Key}");
                }
            }

            return _data;
        }

        private void EnsureSettingsLoaded()
        {
            if (_settings != null)
                return;

            _settings = _adventureDefinitionsManager?.LocalizationSettings;
            if (_settings == null)
                throw new InvalidOperationException("[LocalizationManager] LocalizationSettingsDef is not loaded in Adventure DefinitionsManager.");

            if (_settings.LanguageFolders == null)
                _settings.LanguageFolders = new Dictionary<SystemLanguage, string>();
        }
    }
}
