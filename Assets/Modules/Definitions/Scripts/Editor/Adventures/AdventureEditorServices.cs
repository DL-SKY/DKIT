using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Choice.Actions;
using Modules.RPG.Scripts.Adventure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static Modules.Definitions.Scripts.Implementation.Adventures.Constants.Glossary;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureGraphData
    {
        public readonly List<string> Nodes = new List<string>();
        public readonly List<AdventureGraphEdge> Edges = new List<AdventureGraphEdge>();
    }

    public sealed class AdventureGraphEdge
    {
        public string FromSceneId;
        public string ToSceneId;
        public string ChoiceId;
        public string ChoiceText;
    }

    public sealed class AdventureGraphBuilder
    {
        public AdventureGraphData Build(AdventureData adventureData)
        {
            AdventureGraphData graphData = new AdventureGraphData();
            if (adventureData?.Scenes == null)
                return graphData;

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                string sceneId = pair.Key;
                if (string.IsNullOrWhiteSpace(sceneId))
                    continue;

                graphData.Nodes.Add(sceneId);
                SceneData sceneData = pair.Value;
                if (sceneData?.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                {
                    var choice = sceneData.Choices[choiceIndex];
                    if (choice?.Actions == null)
                        continue;

                    for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                    {
                        var action = choice.Actions[actionIndex];
                        if (action == null || !IsSceneTransitionAction(action))
                            continue;

                        string targetSceneId = GetSceneId(action);
                        if (string.IsNullOrWhiteSpace(targetSceneId))
                            continue;

                        graphData.Edges.Add(new AdventureGraphEdge
                        {
                            FromSceneId = sceneId,
                            ToSceneId = targetSceneId,
                            ChoiceId = choice.Id,
                            ChoiceText = choice.Text,
                        });
                    }
                }
            }

            graphData.Nodes.Sort(StringComparer.Ordinal);
            return graphData;
        }

        private const string LEGACY_SCENE_ID_KEY = "sceneId";

        public static bool IsSceneTransitionAction(ChoiceActionData actionData)
        {
            if (actionData == null)
                return false;

            return actionData.Type == ChoiceActionType.None
                || actionData.Type == ChoiceActionType.GoToScene;
        }

        public static string GetSceneId(ChoiceActionData actionData)
        {
            if (actionData?.Params?.Strings == null)
                return string.Empty;

            if (actionData.Params.Strings.TryGetValue(ChoiceActions.SCENE_ID, out string value)
                && !string.IsNullOrWhiteSpace(value))
                return value;

            return actionData.Params.Strings.TryGetValue(LEGACY_SCENE_ID_KEY, out value) ? value : string.Empty;
        }

        public static void SetSceneId(ChoiceActionData actionData, string sceneId)
        {
            if (actionData == null)
                return;

            actionData.Type = ChoiceActionType.None;
            actionData.Params ??= new ChoiceActionParamsData();
            actionData.Params.Strings ??= new Dictionary<string, string>();
            actionData.Params.Strings[ChoiceActions.SCENE_ID] = sceneId ?? string.Empty;
            actionData.Params.Strings.Remove(LEGACY_SCENE_ID_KEY);
        }

        public static void NormalizeSceneTransitionAction(ChoiceActionData actionData)
        {
            if (!IsSceneTransitionAction(actionData))
                return;

            SetSceneId(actionData, GetSceneId(actionData));
        }
    }

    public interface IAdventureLocalizationKeyCollector
    {
        List<string> Collect(AdventureData adventureData);
    }

    public sealed class DefaultAdventureLocalizationKeyCollector : IAdventureLocalizationKeyCollector
    {
        private const string LOC_PREFIX = "loc:";

        public List<string> Collect(AdventureData adventureData)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            if (adventureData == null)
                return new List<string>();

            CollectFromText(adventureData.Title, keys);
            CollectFromText(adventureData.Description, keys);

            if (adventureData.Scenes == null)
                return ToOrderedList(keys);

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                SceneData sceneData = pair.Value;
                if (sceneData == null)
                    continue;

                if (sceneData.Content != null)
                {
                    for (int contentIndex = 0; contentIndex < sceneData.Content.Count; contentIndex++)
                        CollectFromText(sceneData.Content[contentIndex]?.Value, keys);
                }

                if (sceneData.Choices == null)
                    continue;

                for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                {
                    var choice = sceneData.Choices[choiceIndex];
                    if (choice == null)
                        continue;

                    CollectFromText(choice.Text, keys);
                    CollectFromText(choice.Description, keys);

                    if (choice.Actions == null)
                        continue;

                    for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                    {
                        var action = choice.Actions[actionIndex];
                        if (action?.Params?.Strings == null)
                            continue;

                        foreach (var stringParam in action.Params.Strings)
                            CollectFromText(stringParam.Value, keys);
                    }
                }
            }

            return ToOrderedList(keys);
        }

        private static void CollectFromText(string value, HashSet<string> keys)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            string trimmed = value.Trim();
            if (!trimmed.StartsWith(LOC_PREFIX, StringComparison.OrdinalIgnoreCase))
                return;

            string key = trimmed.Substring(LOC_PREFIX.Length).Trim();
            if (!string.IsNullOrWhiteSpace(key))
                keys.Add(key);
        }

        private static List<string> ToOrderedList(HashSet<string> keys)
        {
            List<string> result = new List<string>(keys);
            result.Sort(StringComparer.Ordinal);
            return result;
        }
    }

    public sealed class LocalizationExportEntry
    {
        public string Key;
        public string Text;
    }

    public sealed class AdventureLocalizationGenerationResult
    {
        public readonly List<LocalizationExportEntry> ExportEntries = new List<LocalizationExportEntry>();
        public string ExportFilePath;
        public int UpdatedFieldsCount;
        public int GeneratedKeysCount;
        public int ReusedKeysCount;
    }

    public sealed class AdventureLocalizationGenerationService
    {
        private const string LOC_PREFIX = "loc:";
        private const int MAX_TOKEN_LENGTH = 20;
        private static readonly Regex KEY_REGEX = new Regex(
            "^[A-Za-z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)*$",
            RegexOptions.Compiled);
        private static readonly Regex ARG_PLACEHOLDER_REGEX = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);
        private static readonly Regex CAMEL_CASE_SPLIT_REGEX = new Regex("([a-z0-9])([A-Z])", RegexOptions.Compiled);
        private static readonly Regex NON_ALNUM_REGEX = new Regex("[^A-Za-z0-9]+", RegexOptions.Compiled);
        private static readonly Regex MULTI_UNDERSCORE_REGEX = new Regex("_{2,}", RegexOptions.Compiled);

        public AdventureLocalizationGenerationResult GenerateAndExport(
            AdventureData adventureData,
            string selectedAdventurePath,
            string exportDirectory)
        {
            if (adventureData == null)
                throw new ArgumentNullException(nameof(adventureData));

            if (string.IsNullOrWhiteSpace(exportDirectory))
                throw new ArgumentException("Export directory is empty.", nameof(exportDirectory));

            if (!Directory.Exists(exportDirectory))
                throw new DirectoryNotFoundException($"Directory not found: {exportDirectory}");

            AdventureLocalizationGenerationResult result = new AdventureLocalizationGenerationResult();
            HashSet<string> usedKeys = CollectAlreadyUsedKeys(adventureData);
            Dictionary<string, string> textToGeneratedKey = new Dictionary<string, string>(StringComparer.Ordinal);
            string adventurePrefix = BuildAdventurePrefix(adventureData, selectedAdventurePath);

            ProcessField(
                ref adventureData.Title,
                $"{adventurePrefix}_ADV_TITLE",
                textToGeneratedKey,
                usedKeys,
                result);

            ProcessField(
                ref adventureData.Description,
                $"{adventurePrefix}_ADV_DESCR",
                textToGeneratedKey,
                usedKeys,
                result);

            if (adventureData.Scenes != null)
            {
                List<string> orderedSceneIds = new List<string>(adventureData.Scenes.Keys);
                orderedSceneIds.Sort(StringComparer.Ordinal);
                for (int sceneIndex = 0; sceneIndex < orderedSceneIds.Count; sceneIndex++)
                {
                    string sceneId = orderedSceneIds[sceneIndex];
                    if (!adventureData.Scenes.TryGetValue(sceneId, out SceneData sceneData) || sceneData == null)
                        continue;

                    string sceneToken = BuildToken(sceneId, "SCENE");
                    if (sceneData.Content != null)
                    {
                        for (int contentIndex = 0; contentIndex < sceneData.Content.Count; contentIndex++)
                        {
                            SceneContentData content = sceneData.Content[contentIndex];
                            if (content == null || content.Type != SceneContentType.Text)
                                continue;

                            ProcessField(
                                ref content.Value,
                                $"{adventurePrefix}_{sceneToken}_CNT_{contentIndex + 1}_TEXT",
                                textToGeneratedKey,
                                usedKeys,
                                result);
                        }
                    }

                    if (sceneData.Choices == null)
                        continue;

                    for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                    {
                        var choice = sceneData.Choices[choiceIndex];
                        if (choice == null)
                            continue;

                        string choiceBase = $"{adventurePrefix}_{sceneToken}_CH_{choiceIndex + 1}";
                        ProcessField(
                            ref choice.Text,
                            $"{choiceBase}_TEXT",
                            textToGeneratedKey,
                            usedKeys,
                            result);

                        ProcessField(
                            ref choice.Description,
                            $"{choiceBase}_DESCR",
                            textToGeneratedKey,
                            usedKeys,
                            result);
                    }
                }
            }

            result.ExportFilePath = WriteExportFile(exportDirectory, adventurePrefix, result.ExportEntries);
            return result;
        }

        private static void ProcessField(
            ref string fieldValue,
            string baseKey,
            Dictionary<string, string> textToGeneratedKey,
            HashSet<string> usedKeys,
            AdventureLocalizationGenerationResult result)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return;

            string rawValue = fieldValue.Trim();
            if (TryExtractKey(rawValue, out string existingKey))
            {
                usedKeys.Add(existingKey);
                return;
            }

            if (textToGeneratedKey.TryGetValue(rawValue, out string reusedKey))
            {
                fieldValue = reusedKey;
                result.ReusedKeysCount++;
                result.UpdatedFieldsCount++;
                return;
            }

            int argumentCount = CountUniqueArguments(rawValue);
            string keyedBase = argumentCount > 0 ? $"{baseKey}_ARG_{argumentCount}" : baseKey;
            string generatedKey = BuildUniqueKey(keyedBase, usedKeys);

            fieldValue = generatedKey;
            textToGeneratedKey[rawValue] = generatedKey;
            usedKeys.Add(generatedKey);

            result.GeneratedKeysCount++;
            result.UpdatedFieldsCount++;
            result.ExportEntries.Add(new LocalizationExportEntry
            {
                Key = generatedKey,
                Text = NormalizeTextForTsv(rawValue),
            });
        }

        private static HashSet<string> CollectAlreadyUsedKeys(AdventureData adventureData)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            CollectKey(adventureData?.Title, result);
            CollectKey(adventureData?.Description, result);

            if (adventureData?.Scenes == null)
                return result;

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                SceneData sceneData = pair.Value;
                if (sceneData?.Choices == null)
                    continue;

                for (int i = 0; i < sceneData.Choices.Count; i++)
                {
                    var choice = sceneData.Choices[i];
                    if (choice == null)
                        continue;

                    CollectKey(choice.Text, result);
                    CollectKey(choice.Description, result);
                }
            }

            return result;
        }

        private static void CollectKey(string value, HashSet<string> result)
        {
            if (TryExtractKey(value, out string key))
                result.Add(key);
        }

        private static bool TryExtractKey(string value, out string key)
        {
            key = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string trimmed = value.Trim();
            string candidate = trimmed.StartsWith(LOC_PREFIX, StringComparison.OrdinalIgnoreCase)
                ? trimmed.Substring(LOC_PREFIX.Length).Trim()
                : trimmed;

            if (!KEY_REGEX.IsMatch(candidate))
                return false;

            key = candidate;
            return true;
        }

        private static int CountUniqueArguments(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            HashSet<int> indices = new HashSet<int>();
            MatchCollection matches = ARG_PLACEHOLDER_REGEX.Matches(value);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                if (match.Groups.Count < 2)
                    continue;

                if (int.TryParse(match.Groups[1].Value, out int index))
                    indices.Add(index);
            }

            return indices.Count;
        }

        private static string BuildUniqueKey(string baseKey, HashSet<string> usedKeys)
        {
            string sanitizedBase = BuildKeyCandidate(baseKey);
            if (!usedKeys.Contains(sanitizedBase))
                return sanitizedBase;

            int suffix = 2;
            while (true)
            {
                string candidate = $"{sanitizedBase}_{suffix}";
                if (!usedKeys.Contains(candidate))
                    return candidate;

                suffix++;
            }
        }

        private static string BuildKeyCandidate(string source)
        {
            string prepared = CAMEL_CASE_SPLIT_REGEX.Replace(source ?? string.Empty, "$1_$2");
            prepared = NON_ALNUM_REGEX.Replace(prepared, "_");
            prepared = MULTI_UNDERSCORE_REGEX.Replace(prepared, "_").Trim('_');
            if (string.IsNullOrWhiteSpace(prepared))
                prepared = "LOC_KEY";

            return prepared.ToUpperInvariant();
        }

        private static string BuildAdventurePrefix(AdventureData adventureData, string selectedAdventurePath)
        {
            string source = Path.GetFileNameWithoutExtension(selectedAdventurePath);
            if (string.IsNullOrWhiteSpace(source))
                source = adventureData?.Id;

            return BuildToken(source, "ADV");
        }

        private static string BuildToken(string source, string fallback)
        {
            if (string.IsNullOrWhiteSpace(source))
                return fallback;

            string prepared = CAMEL_CASE_SPLIT_REGEX.Replace(source, "$1_$2");
            prepared = NON_ALNUM_REGEX.Replace(prepared, "_");
            prepared = MULTI_UNDERSCORE_REGEX.Replace(prepared, "_").Trim('_');
            if (string.IsNullOrWhiteSpace(prepared))
                return fallback;

            prepared = prepared.ToUpperInvariant();
            if (prepared.Length > MAX_TOKEN_LENGTH)
                prepared = prepared.Substring(0, MAX_TOKEN_LENGTH);

            return prepared;
        }

        private static string NormalizeTextForTsv(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();
        }

        private static string WriteExportFile(string directory, string adventurePrefix, List<LocalizationExportEntry> entries)
        {
            string fileName = $"{adventurePrefix}_localization_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(directory, fileName);

            List<string> lines = new List<string>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                LocalizationExportEntry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                    continue;

                lines.Add($"{entry.Key}\t{entry.Text ?? string.Empty}");
            }

            File.WriteAllLines(filePath, lines, new UTF8Encoding(false));
            return filePath;
        }
    }

    public sealed class AdventureValidationService
    {
        public List<string> Validate(AdventureData adventureData)
        {
            List<string> errors = new List<string>();
            if (adventureData == null)
            {
                errors.Add("Adventure is null.");
                return errors;
            }

            if (adventureData.Scenes == null || adventureData.Scenes.Count == 0)
                errors.Add("Adventure must contain at least one scene.");

            if (adventureData.StartScenes == null || adventureData.StartScenes.Count == 0)
                errors.Add("Adventure must contain at least one start scene.");

            if (adventureData.StartScenes != null && adventureData.Scenes != null)
            {
                for (int i = 0; i < adventureData.StartScenes.Count; i++)
                {
                    string startSceneId = adventureData.StartScenes[i];
                    if (string.IsNullOrWhiteSpace(startSceneId))
                    {
                        errors.Add("Start scene id cannot be empty.");
                        continue;
                    }

                    if (!adventureData.Scenes.ContainsKey(startSceneId))
                        errors.Add($"Start scene '{startSceneId}' does not exist in Scenes.");
                }
            }

            if (adventureData.Scenes != null)
            {
                foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
                {
                    string sceneId = pair.Key;
                    SceneData sceneData = pair.Value;

                    if (string.IsNullOrWhiteSpace(sceneId))
                        errors.Add("Scene dictionary contains empty scene id.");

                    if (sceneData == null)
                    {
                        errors.Add($"Scene '{sceneId}' is null.");
                        continue;
                    }

                    if (!string.Equals(sceneData.Id, sceneId, StringComparison.Ordinal))
                        errors.Add($"Scene key '{sceneId}' does not match SceneData.Id '{sceneData.Id}'.");

                    if (sceneData.Choices == null)
                        continue;

                    HashSet<string> choiceIds = new HashSet<string>(StringComparer.Ordinal);
                    for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                    {
                        var choice = sceneData.Choices[choiceIndex];
                        if (choice == null)
                        {
                            errors.Add($"Scene '{sceneId}' contains null choice.");
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(choice.Id) && !choiceIds.Add(choice.Id))
                            errors.Add($"Scene '{sceneId}' contains duplicated choice id '{choice.Id}'.");

                        if (choice.Actions == null)
                            continue;

                        for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                        {
                            var action = choice.Actions[actionIndex];
                            if (action == null)
                            {
                                errors.Add($"Scene '{sceneId}' contains null action in choice '{choice.Id}'.");
                                continue;
                            }

                            if (!AdventureGraphBuilder.IsSceneTransitionAction(action))
                                continue;

                            string targetSceneId = AdventureGraphBuilder.GetSceneId(action);
                            if (string.IsNullOrWhiteSpace(targetSceneId))
                            {
                                errors.Add($"Choice '{choice.Id}' in scene '{sceneId}' has scene transition action with empty SceneId.");
                                continue;
                            }

                            if (adventureData.Scenes != null && !adventureData.Scenes.ContainsKey(targetSceneId))
                                errors.Add($"Choice '{choice.Id}' in scene '{sceneId}' points to missing scene '{targetSceneId}'.");
                        }
                    }
                }
            }

            return errors;
        }
    }
}
