using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    public static class SpaceDefinitionLoader
    {
        public const string HULLS_PATH = "Definitions/Space/Hulls";
        public const string MODULES_PATH = "Definitions/Space/Modules";
        public const string COMMANDS_PATH = "Definitions/Space/Commands";
        public const string TIMED_TARGETS_PATH = "Definitions/Space/TimedTargets";

        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        public static Dictionary<string, T> Load<T>(string resourcesPath) where T : AbstractDefinition
        {
            var result = new Dictionary<string, T>();
            var assets = Resources.LoadAll<TextAsset>(resourcesPath);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (asset == null || string.IsNullOrWhiteSpace(asset.text))
                    continue;

                var definition = JsonConvert.DeserializeObject<T>(asset.text, SETTINGS);
                if (definition == null)
                {
                    UnityEngine.Debug.LogWarning($"[Space] Definition '{asset.name}' in '{resourcesPath}' did not deserialize.");
                    continue;
                }

                definition.Id = asset.name;
                if (!result.TryAdd(definition.Id, definition))
                    UnityEngine.Debug.LogWarning($"[Space] Duplicate definition id '{definition.Id}' in '{resourcesPath}'.");
            }

            return result;
        }
    }
}
