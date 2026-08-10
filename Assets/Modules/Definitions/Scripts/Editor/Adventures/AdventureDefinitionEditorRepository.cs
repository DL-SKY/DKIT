using Modules.Definitions.Scripts.Defs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureDefinitionEditorRepository
    {
        private const string JSON_EXTENSION = ".json";
        private static readonly JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter
                {
                    AllowIntegerValues = true,
                },
            },
        };

        public List<string> GetDefinitionFiles(string definitionsDirectory)
        {
            string directoryPath = ToAbsolutePath(definitionsDirectory);
            EnsureDirectoryExists(directoryPath);

            string[] files = Directory.GetFiles(directoryPath, "*" + JSON_EXTENSION, SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            List<string> result = new List<string>(files.Length);
            for (int i = 0; i < files.Length; i++)
                result.Add(ToProjectRelativePath(files[i]));

            return result;
        }

        public Dictionary<string, TDefinition> LoadAllDefinitions<TDefinition>(string definitionsDirectory)
            where TDefinition : AbstractDefinition
        {
            Dictionary<string, TDefinition> result = new Dictionary<string, TDefinition>(StringComparer.Ordinal);
            List<string> files = GetDefinitionFiles(definitionsDirectory);

            for (int i = 0; i < files.Count; i++)
            {
                if (!TryLoad(files[i], out TDefinition definition, out _))
                    continue;

                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                if (result.ContainsKey(definition.Id))
                    continue;

                result[definition.Id] = definition;
            }

            return result;
        }

        public bool TryLoad<TDefinition>(
            string projectRelativePath,
            out TDefinition definition,
            out string error)
            where TDefinition : AbstractDefinition
        {
            definition = null;
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
                definition = JsonConvert.DeserializeObject<TDefinition>(content, JSON_SETTINGS);
                if (definition == null)
                {
                    error = $"Failed to deserialize '{typeof(TDefinition).Name}'.";
                    return false;
                }

                definition.Id = GetFileNameWithoutExtension(projectRelativePath);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        public void Save<TDefinition>(string projectRelativePath, TDefinition definition)
            where TDefinition : AbstractDefinition
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            string absolutePath = ToAbsolutePath(projectRelativePath);
            string directoryPath = Path.GetDirectoryName(absolutePath);
            EnsureDirectoryExists(directoryPath);

            string json = JsonConvert.SerializeObject(definition, Formatting.Indented, JSON_SETTINGS);
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

        public string BuildProjectRelativePath(string directoryPath, string definitionId)
        {
            string safeId = NormalizeDefinitionId(definitionId);
            return $"{directoryPath}/{safeId}{JSON_EXTENSION}";
        }

        public string NormalizeDefinitionId(string value)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return "NewDefinition";

            normalized = normalized.Replace(JSON_EXTENSION, string.Empty, StringComparison.OrdinalIgnoreCase);
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                normalized = normalized.Replace(invalidChar, '_');

            normalized = normalized.Replace(" ", string.Empty);
            return string.IsNullOrWhiteSpace(normalized) ? "NewDefinition" : normalized;
        }

        public static string GetFileNameWithoutExtension(string projectRelativePath)
        {
            return Path.GetFileNameWithoutExtension(projectRelativePath);
        }

        private static string ToAbsolutePath(string projectRelativePath)
        {
            return Path.GetFullPath(projectRelativePath);
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
    }
}
