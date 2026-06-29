using Modules.Cheats.Scripts.Editor.Core;
using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Localization.Scripts.Implementation;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Modules.Cheats.Scripts.Editor.Implementation.Localization
{
    public sealed class LocalizationCheatSection : CheatSectionBase
    {
        private string _lastActionMessage;

        public override string Id => "Localization";

        public override int Order => -997;

        public override bool IsVisible(string filter)
        {
            return base.IsVisible(filter);
        }

        public override void DrawContent()
        {
            var localizationManager = TryGetLocalizationManager();
            if (localizationManager == null)
            {
                EditorGUILayout.HelpBox("Нет LocalizationManager.", MessageType.Info);
                return;
            }

            var definitionsManager = TryGetAdventuresDefinitionsManager();
            if (definitionsManager?.LocalizationSettings == null)
            {
                EditorGUILayout.HelpBox("Нет LocalizationSettingsDef в Adventures.DefinitionsManager.", MessageType.Info);
                return;
            }

            DrawSectionHeader("LocalizationManager:");
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Current language: " + localizationManager.Language);

            DrawLanguageButtons(localizationManager, definitionsManager);

            if (!string.IsNullOrWhiteSpace(_lastActionMessage))
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox(_lastActionMessage, MessageType.Info);
            }
        }

        private void DrawLanguageButtons(LocalizationManager localizationManager, DefinitionsManager definitionsManager)
        {
            var settings = definitionsManager.LocalizationSettings;
            if (settings.LanguageFolders == null || settings.LanguageFolders.Count == 0)
            {
                EditorGUILayout.HelpBox("В LocalizationSettingsDef нет доступных языков.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Set language:", EditorStyles.boldLabel);

            var languages = new System.Collections.Generic.List<SystemLanguage>(settings.LanguageFolders.Keys);
            languages.Sort((left, right) => string.Compare(left.ToString(), right.ToString(), System.StringComparison.Ordinal));

            for (var i = 0; i < languages.Count; i++)
            {
                var language = languages[i];
                if (GUILayout.Button(language.ToString(), GUILayout.Height(22f)))
                {
                    ApplyLanguage(localizationManager, language);
                }
            }
        }

        private void ApplyLanguage(LocalizationManager localizationManager, SystemLanguage language)
        {
            _lastActionMessage = string.Empty;

            if (!localizationManager.TrySetLanguage(language))
            {
                _lastActionMessage = "Не удалось применить язык: " + language;
                return;
            }

            var stateLogic = TryGetAdventureStateLogic();
            if (stateLogic == null)
            {
                _lastActionMessage = "Язык применен, но AdventureStateLogic не найден (state не обновлен).";
                return;
            }

            var result = stateLogic.ProcessAction(new SetLocalizationLanguageStateAction<StateData>(language), true);
            if (!result.IsValid)
            {
                _lastActionMessage = "Язык применен, но state action завершился ошибкой: " + result.ErrorMessage;
                return;
            }

            _lastActionMessage = "Язык применен и сохранен в state: " + language;
        }

        private static LocalizationManager TryGetLocalizationManager()
        {
            if (!Application.isPlaying)
                return null;

            if (!ProjectContext.HasInstance)
                return null;

            return ProjectContext.Instance.Container.TryResolve<LocalizationManager>();
        }

        private static DefinitionsManager TryGetAdventuresDefinitionsManager()
        {
            if (!Application.isPlaying || !ProjectContext.HasInstance)
                return null;

            return ProjectContext.Instance.Container.TryResolve<DefinitionsManager>();
        }

        private static AdventureStateLogic TryGetAdventureStateLogic()
        {
            if (!Application.isPlaying || !ProjectContext.HasInstance)
                return null;

            return ProjectContext.Instance.Container.TryResolve<AdventureStateLogic>();
        }
    }
}
