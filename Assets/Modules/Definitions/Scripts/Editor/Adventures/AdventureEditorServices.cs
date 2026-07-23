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
        private const int LEGACY_GO_TO_SCENE_TYPE_CODE = 100;
        private const string LEGACY_SCENE_ID_KEY = "sceneId";

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

        public static bool IsSceneTransitionAction(ChoiceActionData actionData)
        {
            if (actionData == null)
                return false;

            int typeCode = (int)actionData.Type;
            return actionData.Type == ChoiceActionType.GoToScene
                || actionData.Type == ChoiceActionType.None
                || typeCode == LEGACY_GO_TO_SCENE_TYPE_CODE;
        }

        public static string GetSceneId(ChoiceActionData actionData)
        {
            if (actionData?.Params?.Strings == null)
                return string.Empty;

            if (actionData.Params.Strings.TryGetValue(ChoiceActions.SCENE_ID, out string value)
                && !string.IsNullOrWhiteSpace(value))
                return value;

            if (actionData.Params.Strings.TryGetValue(LEGACY_SCENE_ID_KEY, out value)
                && !string.IsNullOrWhiteSpace(value))
                return value;

            return string.Empty;
        }

        public static void SetSceneId(ChoiceActionData actionData, string sceneId)
        {
            if (actionData == null)
                return;

            actionData.Type = ChoiceActionType.GoToScene;
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

    public sealed class AdventureValidationIssue
    {
        private readonly Action _fixAction;

        public string Message { get; }
        public bool CanFix => _fixAction != null;

        public AdventureValidationIssue(string message, Action fixAction = null)
        {
            Message = message ?? string.Empty;
            _fixAction = fixAction;
        }

        public bool ApplyFix()
        {
            if (_fixAction == null)
                return false;

            _fixAction.Invoke();
            return true;
        }
    }

    public sealed class AdventureValidationService
    {
        private enum IdentifierStyle
        {
            Unknown = 0,
            LowerSnake = 1,
            CamelOrPascal = 2,
            Other = 3,
        }

        private static readonly Regex UPPER_SNAKE_TAG_REGEX = new Regex(
            "^[A-Z0-9]+(?:_[A-Z0-9]+)*$",
            RegexOptions.Compiled);
        private static readonly Regex KEY_WITH_UNDERSCORE_REGEX = new Regex(
            "^[A-Za-z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$",
            RegexOptions.Compiled);
        private static readonly Regex LOWER_SNAKE_ID_REGEX = new Regex(
            "^[a-z0-9]+(?:_[a-z0-9]+)*$",
            RegexOptions.Compiled);
        private static readonly Regex CAMEL_OR_PASCAL_ID_REGEX = new Regex(
            "^[A-Za-z][A-Za-z0-9]*$",
            RegexOptions.Compiled);
        private const string LOC_PREFIX = "loc:";

        private sealed class ChoiceActionValidationContract
        {
            public readonly ChoiceActionType Type;
            public readonly List<string> RequiredStringKeys;
            public readonly List<string> RequiredIntKeys;
            public readonly List<string> RequiredBoolKeys;
            public readonly Dictionary<string, string> LegacyStringAliases;
            public readonly bool AllowAnyStringKeys;
            public readonly bool AllowAnyIntKeys;
            public readonly bool AllowAnyBoolKeys;
            public readonly bool RequireAnyParam;

            public ChoiceActionValidationContract(
                ChoiceActionType type,
                List<string> requiredStringKeys,
                List<string> requiredIntKeys,
                List<string> requiredBoolKeys,
                Dictionary<string, string> legacyStringAliases = null,
                bool allowAnyStringKeys = false,
                bool allowAnyIntKeys = false,
                bool allowAnyBoolKeys = false,
                bool requireAnyParam = false)
            {
                Type = type;
                RequiredStringKeys = requiredStringKeys ?? new List<string>();
                RequiredIntKeys = requiredIntKeys ?? new List<string>();
                RequiredBoolKeys = requiredBoolKeys ?? new List<string>();
                LegacyStringAliases = legacyStringAliases ?? new Dictionary<string, string>(StringComparer.Ordinal);
                AllowAnyStringKeys = allowAnyStringKeys;
                AllowAnyIntKeys = allowAnyIntKeys;
                AllowAnyBoolKeys = allowAnyBoolKeys;
                RequireAnyParam = requireAnyParam;
            }

            public bool IsSingleStringKeyContract =>
                RequiredStringKeys.Count == 1
                && RequiredIntKeys.Count == 0
                && RequiredBoolKeys.Count == 0;
        }

        public List<string> Validate(AdventureData adventureData)
        {
            List<AdventureValidationIssue> issues = ValidateDetailed(adventureData);
            List<string> messages = new List<string>(issues.Count);
            for (int i = 0; i < issues.Count; i++)
                messages.Add(issues[i].Message);

            return messages;
        }

        public List<AdventureValidationIssue> ValidateDetailed(AdventureData adventureData)
        {
            List<AdventureValidationIssue> issues = new List<AdventureValidationIssue>();
            if (adventureData == null)
            {
                issues.Add(new AdventureValidationIssue("Adventure is null."));
                return issues;
            }

            if (adventureData.Scenes == null || adventureData.Scenes.Count == 0)
                issues.Add(new AdventureValidationIssue("Adventure must contain at least one scene."));

            if (adventureData.StartScenes == null || adventureData.StartScenes.Count == 0)
                issues.Add(new AdventureValidationIssue("Adventure must contain at least one start scene."));

            ValidateTagsList(issues, adventureData.Tags, "Adventure.Tags");
            ValidateTagsList(issues, adventureData.IgnoredTags, "Adventure.IgnoredTags");
            ValidateLocalizedTextField(
                issues,
                "Adventure.Title",
                () => adventureData.Title,
                value => adventureData.Title = value);
            ValidateLocalizedTextField(
                issues,
                "Adventure.Description",
                () => adventureData.Description,
                value => adventureData.Description = value);

            if (adventureData.StartScenes != null && adventureData.Scenes != null)
            {
                for (int i = 0; i < adventureData.StartScenes.Count; i++)
                {
                    string startSceneId = adventureData.StartScenes[i];
                    if (string.IsNullOrWhiteSpace(startSceneId))
                    {
                        issues.Add(new AdventureValidationIssue("Start scene id cannot be empty."));
                        continue;
                    }

                    if (!adventureData.Scenes.ContainsKey(startSceneId))
                        issues.Add(new AdventureValidationIssue($"Start scene '{startSceneId}' does not exist in Scenes."));
                }
            }

            if (adventureData.Scenes != null)
            {
                foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
                {
                    string sceneId = pair.Key;
                    SceneData sceneData = pair.Value;

                    if (string.IsNullOrWhiteSpace(sceneId))
                        issues.Add(new AdventureValidationIssue("Scene dictionary contains empty scene id."));

                    if (sceneData == null)
                    {
                        issues.Add(new AdventureValidationIssue($"Scene '{sceneId}' is null."));
                        continue;
                    }

                    if (!string.Equals(sceneData.Id, sceneId, StringComparison.Ordinal))
                        issues.Add(new AdventureValidationIssue($"Scene key '{sceneId}' does not match SceneData.Id '{sceneData.Id}'."));

                    ValidateTagsList(issues, sceneData.Tags, $"Scene '{sceneId}'.Tags");

                    if (sceneData.Content != null)
                    {
                        for (int contentIndex = 0; contentIndex < sceneData.Content.Count; contentIndex++)
                        {
                            SceneContentData contentData = sceneData.Content[contentIndex];
                            if (contentData == null)
                                continue;

                            ValidateLocalizedTextField(
                                issues,
                                $"Scene '{sceneId}' content #{contentIndex} Value",
                                () => contentData.Value,
                                value => contentData.Value = value);
                        }
                    }

                    if (sceneData.Choices == null)
                        continue;

                    HashSet<string> choiceIds = new HashSet<string>(StringComparer.Ordinal);
                    for (int choiceIndex = 0; choiceIndex < sceneData.Choices.Count; choiceIndex++)
                    {
                        ChoiceData choice = sceneData.Choices[choiceIndex];
                        if (choice == null)
                        {
                            issues.Add(new AdventureValidationIssue($"Scene '{sceneId}' contains null choice."));
                            continue;
                        }

                        string choiceId = string.IsNullOrWhiteSpace(choice.Id) ? $"choice_{choiceIndex}" : choice.Id;

                        if (!string.IsNullOrWhiteSpace(choice.Id) && !choiceIds.Add(choice.Id))
                            issues.Add(new AdventureValidationIssue($"Scene '{sceneId}' contains duplicated choice id '{choice.Id}'."));

                        ValidateTagsList(issues, choice.Tags, $"Choice '{choiceId}' in scene '{sceneId}'.Tags");
                        ValidateLocalizedTextField(
                            issues,
                            $"Choice '{choiceId}' in scene '{sceneId}' Text",
                            () => choice.Text,
                            value => choice.Text = value);
                        ValidateLocalizedTextField(
                            issues,
                            $"Choice '{choiceId}' in scene '{sceneId}' Description",
                            () => choice.Description,
                            value => choice.Description = value);

                        if (choice.Type == ChoiceType.Default)
                        {
                            if (choice.DiceCheck != null)
                            {
                                issues.Add(new AdventureValidationIssue(
                                    $"Choice '{choiceId}' in scene '{sceneId}' has type '{ChoiceType.Default}' but DiceCheck block is set. DiceCheck must be null for Default.",
                                    () => choice.DiceCheck = null));
                            }
                        }
                        else if (choice.Type == ChoiceType.DiceCheck)
                        {
                            if (choice.DiceCheck == null)
                            {
                                issues.Add(new AdventureValidationIssue(
                                    $"Choice '{choiceId}' in scene '{sceneId}' has type '{ChoiceType.DiceCheck}' but DiceCheck block is missing.",
                                    () =>
                                    {
                                        choice.DiceCheck = new ChoiceDiceCheckData
                                        {
                                            DifficultyClass = 15,
                                            DiceType = Modules.Dices.Scripts.DiceType.D20,
                                            DiceOptions = Modules.Dices.Scripts.DiceOptions.None,
                                            DiceCheckParam = string.Empty,
                                            OnCriticalSuccess = new List<ChoiceActionData>(),
                                            OnSuccess = new List<ChoiceActionData>(),
                                            OnFailure = new List<ChoiceActionData>(),
                                            OnCriticalFailure = new List<ChoiceActionData>(),
                                        };
                                    }));
                            }
                            else if (choice.DiceCheck.DifficultyClass < 0)
                            {
                                issues.Add(new AdventureValidationIssue(
                                    $"Choice '{choiceId}' in scene '{sceneId}' has negative DiceCheck.DifficultyClass.",
                                    () => choice.DiceCheck.DifficultyClass = 0));
                            }

                            choice.DiceCheck.DiceCheckParam ??= string.Empty;
                            choice.DiceCheck.OnCriticalSuccess ??= new List<ChoiceActionData>();
                            choice.DiceCheck.OnSuccess ??= new List<ChoiceActionData>();
                            choice.DiceCheck.OnFailure ??= new List<ChoiceActionData>();
                            choice.DiceCheck.OnCriticalFailure ??= new List<ChoiceActionData>();

                            if (choice.Actions != null && choice.Actions.Count > 0)
                            {
                                issues.Add(new AdventureValidationIssue(
                                    $"Choice '{choiceId}' in scene '{sceneId}' has type '{ChoiceType.DiceCheck}' but contains {choice.Actions.Count} item(s) in Actions. For DiceCheck, Actions must be null or empty.",
                                    () => choice.Actions.Clear()));
                            }
                        }

                        if (choice.Type != ChoiceType.Default)
                            continue;

                        if (choice.Actions == null)
                            continue;

                        for (int actionIndex = 0; actionIndex < choice.Actions.Count; actionIndex++)
                        {
                            ChoiceActionData action = choice.Actions[actionIndex];
                            if (action == null)
                            {
                                issues.Add(new AdventureValidationIssue($"Scene '{sceneId}' contains null action in choice '{choiceId}'."));
                                continue;
                            }

                            ValidateActionParamsContract(issues, action, sceneId, choiceId, actionIndex);

                            if (!AdventureGraphBuilder.IsSceneTransitionAction(action))
                                continue;

                            string targetSceneId = AdventureGraphBuilder.GetSceneId(action);
                            if (string.IsNullOrWhiteSpace(targetSceneId))
                            {
                                issues.Add(new AdventureValidationIssue(
                                    $"Choice '{choiceId}' in scene '{sceneId}' has scene transition action with empty SceneId."));
                                continue;
                            }

                            if (!adventureData.Scenes.ContainsKey(targetSceneId))
                                issues.Add(new AdventureValidationIssue(
                                    $"Choice '{choiceId}' in scene '{sceneId}' points to missing scene '{targetSceneId}'."));
                        }
                    }
                }
            }

            ValidateIdentifierStyleConsistency(issues, adventureData);
            return issues;
        }

        private static void ValidateTagsList(List<AdventureValidationIssue> issues, List<string> tags, string scope)
        {
            if (tags == null)
                return;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (string.IsNullOrWhiteSpace(tag))
                {
                    int indexCopy = i;
                    issues.Add(new AdventureValidationIssue(
                        $"{scope} contains an empty tag at index {indexCopy}.",
                        () => tags.RemoveAt(indexCopy)));
                    continue;
                }

                if (UPPER_SNAKE_TAG_REGEX.IsMatch(tag))
                    continue;

                string normalizedTag = NormalizeTagToUpperSnake(tag);
                if (string.IsNullOrWhiteSpace(normalizedTag))
                {
                    issues.Add(new AdventureValidationIssue(
                        $"{scope} tag '{tag}' cannot be normalized to UPPER_SNAKE_CASE."));
                    continue;
                }

                int index = i;
                string oldTag = tag;
                issues.Add(new AdventureValidationIssue(
                    $"{scope} tag '{oldTag}' must be UPPER_SNAKE_CASE.",
                    () => tags[index] = normalizedTag));
            }
        }

        private static void ValidateLocalizedTextField(
            List<AdventureValidationIssue> issues,
            string fieldPath,
            Func<string> getter,
            Action<string> setter)
        {
            string value = getter?.Invoke();
            if (string.IsNullOrEmpty(value))
                return;

            if (HasLineBreak(value))
            {
                issues.Add(new AdventureValidationIssue(
                    $"{fieldPath} contains line breaks. Use escaped '\\n' (or RTF tags) instead of literal line breaks.",
                    () => setter?.Invoke(ReplaceLineBreaksWithEscapedNewlines(value))));
            }

            if (!LooksLikeLocalizationKey(value))
                return;

            issues.Add(new AdventureValidationIssue(
                $"{fieldPath} looks like a localization key ('{value}'). TEA adventure JSON should store user-facing text unless explicitly requested otherwise."));
        }

        private static bool HasLineBreak(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0;
        }

        private static string ReplaceLineBreaksWithEscapedNewlines(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Replace("\n", "\\n");
        }

        private static bool LooksLikeLocalizationKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string trimmed = value.Trim();
            if (trimmed.StartsWith(LOC_PREFIX, StringComparison.OrdinalIgnoreCase))
                return true;

            if (trimmed.IndexOf(' ') >= 0)
                return false;

            return KEY_WITH_UNDERSCORE_REGEX.IsMatch(trimmed);
        }

        private static string NormalizeTagToUpperSnake(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToUpperInvariant(c));
                else
                    builder.Append('_');
            }

            string normalized = Regex.Replace(builder.ToString(), "_{2,}", "_").Trim('_');
            return normalized;
        }

        private static void ValidateIdentifierStyleConsistency(List<AdventureValidationIssue> issues, AdventureData adventureData)
        {
            HashSet<IdentifierStyle> sceneStyles = new HashSet<IdentifierStyle>();
            HashSet<IdentifierStyle> choiceStyles = new HashSet<IdentifierStyle>();

            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                string sceneId = pair.Key;
                IdentifierStyle sceneStyle = DetectIdentifierStyle(sceneId);
                if (sceneStyle == IdentifierStyle.Other)
                {
                    issues.Add(new AdventureValidationIssue(
                        $"Scene id '{sceneId}' has an unsupported style. Use either lower_snake_case or CamelCase/PascalCase."));
                }
                else if (sceneStyle != IdentifierStyle.Unknown)
                {
                    sceneStyles.Add(sceneStyle);
                }

                SceneData scene = pair.Value;
                if (scene?.Choices == null)
                    continue;

                for (int i = 0; i < scene.Choices.Count; i++)
                {
                    string choiceId = scene.Choices[i]?.Id;
                    IdentifierStyle choiceStyle = DetectIdentifierStyle(choiceId);
                    if (choiceStyle == IdentifierStyle.Other)
                    {
                        issues.Add(new AdventureValidationIssue(
                            $"Choice id '{choiceId}' in scene '{sceneId}' has an unsupported style. Use either lower_snake_case or CamelCase/PascalCase."));
                    }
                    else if (choiceStyle != IdentifierStyle.Unknown)
                    {
                        choiceStyles.Add(choiceStyle);
                    }
                }
            }

            if (sceneStyles.Count > 1)
            {
                issues.Add(new AdventureValidationIssue(
                    "Scene ids use mixed styles. Keep one style consistently across adventure scenes."));
            }

            if (choiceStyles.Count > 1)
            {
                issues.Add(new AdventureValidationIssue(
                    "Choice ids use mixed styles. Keep one style consistently across adventure choices."));
            }
        }

        private static IdentifierStyle DetectIdentifierStyle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return IdentifierStyle.Unknown;

            if (LOWER_SNAKE_ID_REGEX.IsMatch(value))
                return IdentifierStyle.LowerSnake;

            if (CAMEL_OR_PASCAL_ID_REGEX.IsMatch(value))
                return IdentifierStyle.CamelOrPascal;

            return IdentifierStyle.Other;
        }

        private static void ValidateActionParamsContract(
            List<AdventureValidationIssue> issues,
            ChoiceActionData action,
            string sceneId,
            string choiceId,
            int actionIndex)
        {
            ChoiceActionValidationContract contract = GetChoiceActionContract(action.Type);
            if (contract == null)
            {
                issues.Add(new AdventureValidationIssue(
                    $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} has type '{action.Type}' without TEA validation contract. " +
                    "Add contract validation for this ChoiceActionType and Glossary keys."));
                return;
            }

            action.Params ??= new ChoiceActionParamsData();
            action.Params.Strings ??= new Dictionary<string, string>();
            action.Params.Ints ??= new Dictionary<string, int>();
            action.Params.Bools ??= new Dictionary<string, bool>();

            if (contract.RequireAnyParam
                && action.Params.Strings.Count == 0
                && action.Params.Ints.Count == 0
                && action.Params.Bools.Count == 0)
            {
                issues.Add(new AdventureValidationIssue(
                    $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} of type '{action.Type}' must contain at least one param."));
            }

            ValidateStringKeys(issues, action, contract, sceneId, choiceId, actionIndex);
            ValidateUnexpectedKeys(
                issues,
                action.Params.Ints,
                contract.RequiredIntKeys,
                contract.AllowAnyIntKeys,
                "Ints",
                sceneId,
                choiceId,
                actionIndex);
            ValidateUnexpectedKeys(
                issues,
                action.Params.Bools,
                contract.RequiredBoolKeys,
                contract.AllowAnyBoolKeys,
                "Bools",
                sceneId,
                choiceId,
                actionIndex);
        }

        private static void ValidateStringKeys(
            List<AdventureValidationIssue> issues,
            ChoiceActionData action,
            ChoiceActionValidationContract contract,
            string sceneId,
            string choiceId,
            int actionIndex)
        {
            Dictionary<string, string> strings = action.Params.Strings;

            if (contract.AllowAnyStringKeys)
                return;

            HashSet<string> expected = new HashSet<string>(contract.RequiredStringKeys, StringComparer.Ordinal);

            for (int i = 0; i < contract.RequiredStringKeys.Count; i++)
            {
                string key = contract.RequiredStringKeys[i];
                if (strings.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value))
                    continue;

                if (!contract.IsSingleStringKeyContract)
                {
                    issues.Add(new AdventureValidationIssue(
                        $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} is missing required Strings key '{key}' for type '{action.Type}'."));
                    continue;
                }

                string candidateKey = FindFixCandidateKey(strings, contract, key);
                if (string.IsNullOrWhiteSpace(candidateKey))
                {
                    issues.Add(new AdventureValidationIssue(
                        $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} is missing required Strings key '{key}' for type '{action.Type}'."));
                    continue;
                }

                string issueMessage =
                    $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} uses wrong Strings key '{candidateKey}'. Expected '{key}' for type '{action.Type}'.";
                issues.Add(new AdventureValidationIssue(issueMessage, () =>
                {
                    if (!strings.TryGetValue(candidateKey, out string candidateValue))
                        return;

                    strings[key] = candidateValue;
                    if (!string.Equals(candidateKey, key, StringComparison.Ordinal))
                        strings.Remove(candidateKey);
                }));
            }

            List<string> unexpected = new List<string>();
            foreach (KeyValuePair<string, string> pair in strings)
            {
                if (!expected.Contains(pair.Key))
                    unexpected.Add(pair.Key);
            }

            for (int i = 0; i < unexpected.Count; i++)
            {
                string key = unexpected[i];
                if (contract.IsSingleStringKeyContract && contract.RequiredStringKeys.Count == 1)
                {
                    string expectedKey = contract.RequiredStringKeys[0];
                    issues.Add(new AdventureValidationIssue(
                        $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} contains unexpected Strings key '{key}' for type '{action.Type}'.",
                        () =>
                        {
                            if (!strings.ContainsKey(key))
                                return;

                            if (!strings.ContainsKey(expectedKey))
                                strings[expectedKey] = strings[key];

                            strings.Remove(key);
                        }));
                    continue;
                }

                issues.Add(new AdventureValidationIssue(
                    $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} contains unexpected Strings key '{key}' for type '{action.Type}'."));
            }
        }

        private static void ValidateUnexpectedKeys<TValue>(
            List<AdventureValidationIssue> issues,
            Dictionary<string, TValue> dictionary,
            List<string> expectedKeys,
            bool allowAnyKeys,
            string groupName,
            string sceneId,
            string choiceId,
            int actionIndex)
        {
            if (dictionary == null)
                return;

            if (allowAnyKeys)
                return;

            HashSet<string> expected = new HashSet<string>(expectedKeys ?? new List<string>(), StringComparer.Ordinal);
            foreach (KeyValuePair<string, TValue> pair in dictionary)
            {
                if (expected.Contains(pair.Key))
                    continue;

                issues.Add(new AdventureValidationIssue(
                    $"Choice '{choiceId}' in scene '{sceneId}' action #{actionIndex} contains unexpected {groupName} key '{pair.Key}'."));
            }
        }

        private static string FindFixCandidateKey(
            Dictionary<string, string> strings,
            ChoiceActionValidationContract contract,
            string expectedKey)
        {
            foreach (KeyValuePair<string, string> alias in contract.LegacyStringAliases)
            {
                if (string.Equals(alias.Value, expectedKey, StringComparison.Ordinal)
                    && strings.ContainsKey(alias.Key))
                    return alias.Key;
            }

            if (strings.Count == 1)
            {
                foreach (string key in strings.Keys)
                    return key;
            }

            return string.Empty;
        }

        private static ChoiceActionValidationContract GetChoiceActionContract(ChoiceActionType type)
        {
            switch (type)
            {
                case ChoiceActionType.GoToScene:
                    return new ChoiceActionValidationContract(
                        type,
                        new List<string> { ChoiceActions.SCENE_ID },
                        new List<string>(),
                        new List<string>());
                case ChoiceActionType.GoToAdventure:
                    return new ChoiceActionValidationContract(
                        type,
                        new List<string> { ChoiceActions.ADVENTURE_ID },
                        new List<string>(),
                        new List<string>());
                case ChoiceActionType.GoToRandomAdventure:
                    return new ChoiceActionValidationContract(
                        type,
                        new List<string>(),
                        new List<string>(),
                        new List<string>());
                case ChoiceActionType.GoToRandomScene:
                    return new ChoiceActionValidationContract(
                        type,
                        new List<string> { ChoiceActions.SCENE_ID },
                        new List<string>(),
                        new List<string>());
                case ChoiceActionType.OpenWindow:
                    return new ChoiceActionValidationContract(
                        type,
                        new List<string> { ChoiceActions.WINDOW_ID },
                        new List<string>(),
                        new List<string>());
                case ChoiceActionType.SetWorldParams:
                case ChoiceActionType.SetAdventureParams:
                case ChoiceActionType.SetGlobalParams:
                    return new ChoiceActionValidationContract(
                        type,
                        new List<string>(),
                        new List<string>(),
                        new List<string>(),
                        allowAnyStringKeys: true,
                        allowAnyIntKeys: true,
                        allowAnyBoolKeys: true,
                        requireAnyParam: true);

                default:
                    return null;
            }
        }
    }
}
