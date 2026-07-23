using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureEditorFileRepository
    {
        public const string ADVENTURES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Adventures";
        private const string JSON_EXTENSION = ".json";
        private static readonly JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter
                {
                    // Keep backward compatibility with numeric enum values in old JSON files.
                    AllowIntegerValues = true,
                },
            },
        };

        public List<string> GetAdventureFiles()
        {
            string directoryPath = ToAbsolutePath(ADVENTURES_DIRECTORY);
            EnsureDirectoryExists(directoryPath);

            string[] files = Directory.GetFiles(directoryPath, "*" + JSON_EXTENSION, SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            List<string> result = new List<string>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                string relativePath = ToProjectRelativePath(files[i]);
                result.Add(relativePath);
            }

            return result;
        }

        public bool TryLoad(string projectRelativePath, out AdventureData adventureData, out string error)
        {
            adventureData = null;
            error = string.Empty;

            try
            {
                string absolutePath = ToAbsolutePath(projectRelativePath);
                if (!File.Exists(absolutePath))
                {
                    error = $"File was not found: {projectRelativePath}";
                    return false;
                }

                string content = File.ReadAllText(absolutePath);
                adventureData = JsonConvert.DeserializeObject<AdventureData>(content, JSON_SETTINGS);
                if (adventureData == null)
                {
                    error = "Failed to deserialize AdventureData.";
                    return false;
                }

                NormalizeAdventureData(adventureData);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        public void Save(string projectRelativePath, AdventureData adventureData)
        {
            if (adventureData == null)
                throw new ArgumentNullException(nameof(adventureData));

            NormalizeAdventureData(adventureData);

            string absolutePath = ToAbsolutePath(projectRelativePath);
            string directoryPath = Path.GetDirectoryName(absolutePath);
            EnsureDirectoryExists(directoryPath);

            string json = JsonConvert.SerializeObject(adventureData, Formatting.Indented, JSON_SETTINGS);
            File.WriteAllText(absolutePath, json);
            AssetDatabase.Refresh();
        }

        public bool Exists(string projectRelativePath)
        {
            string absolutePath = ToAbsolutePath(projectRelativePath);
            return File.Exists(absolutePath);
        }

        public void Delete(string projectRelativePath)
        {
            string absolutePath = ToAbsolutePath(projectRelativePath);
            if (File.Exists(absolutePath))
                File.Delete(absolutePath);

            AssetDatabase.Refresh();
        }

        public void Rename(string oldProjectRelativePath, string newProjectRelativePath)
        {
            string oldAbsolutePath = ToAbsolutePath(oldProjectRelativePath);
            string newAbsolutePath = ToAbsolutePath(newProjectRelativePath);

            if (!File.Exists(oldAbsolutePath))
                throw new FileNotFoundException("Source file was not found.", oldAbsolutePath);

            string newDirectoryPath = Path.GetDirectoryName(newAbsolutePath);
            EnsureDirectoryExists(newDirectoryPath);

            if (File.Exists(newAbsolutePath))
                throw new IOException($"Destination already exists: {newProjectRelativePath}");

            File.Move(oldAbsolutePath, newAbsolutePath);
            AssetDatabase.Refresh();
        }

        public static string BuildProjectRelativePath(string fileNameWithoutExtension)
        {
            string safeName = NormalizeFileName(fileNameWithoutExtension);
            return $"{ADVENTURES_DIRECTORY}/{safeName}{JSON_EXTENSION}";
        }

        public static string NormalizeFileName(string fileNameWithoutExtension)
        {
            string value = (fileNameWithoutExtension ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return "NewAdventure";

            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, ' ');

            return ConvertToCamelCaseIdentifier(value);
        }

        public static string GetFileNameWithoutExtension(string projectRelativePath)
        {
            return Path.GetFileNameWithoutExtension(projectRelativePath);
        }

        private static string ToAbsolutePath(string projectRelativePath)
        {
            string fullPath = Path.GetFullPath(projectRelativePath);
            return fullPath;
        }

        private static string ToProjectRelativePath(string absolutePath)
        {
            string projectRoot = Path.GetFullPath(".");
            string relative = Path.GetRelativePath(projectRoot, absolutePath);
            return relative.Replace('\\', '/');
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        private static string ConvertToCamelCaseIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "NewAdventure";

            List<string> tokens = new List<string>();
            char[] chars = value.ToCharArray();
            int tokenStart = -1;

            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                bool isAlphaNumeric = char.IsLetterOrDigit(c);
                if (isAlphaNumeric && tokenStart < 0)
                {
                    tokenStart = i;
                    continue;
                }

                if (!isAlphaNumeric && tokenStart >= 0)
                {
                    tokens.Add(value.Substring(tokenStart, i - tokenStart));
                    tokenStart = -1;
                }
            }

            if (tokenStart >= 0)
                tokens.Add(value.Substring(tokenStart));

            if (tokens.Count == 0)
                return "NewAdventure";

            string result = string.Empty;
            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                string trimmedToken = token.Trim();
                if (trimmedToken.Length == 0)
                    continue;

                char first = char.ToUpperInvariant(trimmedToken[0]);
                result += trimmedToken.Length == 1
                    ? first.ToString()
                    : first + trimmedToken.Substring(1);
            }

            if (string.IsNullOrWhiteSpace(result))
                return "NewAdventure";

            if (!char.IsLetter(result[0]))
                result = "Adventure" + result;

            return result;
        }

        private static void NormalizeAdventureData(AdventureData adventureData)
        {
            adventureData.Tags ??= new List<string>();
            adventureData.IgnoredTags ??= new List<string>();
            adventureData.AdventureLinks ??= new List<string>();
            adventureData.Restrictions ??= new List<Modules.Restrictions.Scripts.Core.Restriction>();
            adventureData.StartScenes ??= new List<string>();
            adventureData.Scenes ??= new Dictionary<string, SceneData>();

            Dictionary<string, SceneData> normalizedScenes = new Dictionary<string, SceneData>(adventureData.Scenes.Count);
            foreach (KeyValuePair<string, SceneData> pair in adventureData.Scenes)
            {
                string sceneId = string.IsNullOrWhiteSpace(pair.Key)
                    ? pair.Value?.Id
                    : pair.Key;

                if (string.IsNullOrWhiteSpace(sceneId))
                    continue;

                SceneData sceneData = pair.Value ?? new SceneData();
                sceneData.Id = sceneId;
                sceneData.Tags ??= new List<string>();
                sceneData.Content ??= new List<SceneContentData>();
                sceneData.Choices ??= new List<ChoiceData>();

                for (int i = 0; i < sceneData.Content.Count; i++)
                {
                    SceneContentData contentData = sceneData.Content[i] ?? new SceneContentData();
                    contentData.Restrictions ??= new List<Modules.Restrictions.Scripts.Core.Restriction>();
                    sceneData.Content[i] = contentData;
                }

                for (int i = 0; i < sceneData.Choices.Count; i++)
                {
                    ChoiceData choiceData = sceneData.Choices[i] ?? new ChoiceData();
                    choiceData.Tags ??= new List<string>();
                    choiceData.Restrictions ??= new List<Modules.Restrictions.Scripts.Core.Restriction>();
                    if (choiceData.Type == ChoiceType.DiceCheck)
                    {
                        choiceData.DiceCheck ??= new ChoiceDiceCheckData
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
                        choiceData.DiceCheck.DiceCheckParam ??= string.Empty;
                        choiceData.DiceCheck.OnCriticalSuccess ??= new List<ChoiceActionData>();
                        choiceData.DiceCheck.OnSuccess ??= new List<ChoiceActionData>();
                        choiceData.DiceCheck.OnFailure ??= new List<ChoiceActionData>();
                        choiceData.DiceCheck.OnCriticalFailure ??= new List<ChoiceActionData>();
                    }
                    choiceData.Actions ??= new List<ChoiceActionData>();

                    for (int actionIndex = 0; actionIndex < choiceData.Actions.Count; actionIndex++)
                    {
                        ChoiceActionData actionData = choiceData.Actions[actionIndex] ?? new ChoiceActionData();
                        actionData.Params ??= new ChoiceActionParamsData();
                        actionData.Params.Strings ??= new Dictionary<string, string>();
                        actionData.Params.Ints ??= new Dictionary<string, int>();
                        actionData.Params.Bools ??= new Dictionary<string, bool>();
                        choiceData.Actions[actionIndex] = actionData;
                    }

                    sceneData.Choices[i] = choiceData;
                }

                normalizedScenes[sceneId] = sceneData;
            }

            adventureData.Scenes = normalizedScenes;
        }
    }
}
