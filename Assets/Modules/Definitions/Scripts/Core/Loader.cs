using Modules.Definitions.Scripts.Defs;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Definitions.Scripts.Core
{
    public sealed class Loader
    {
        public T LoadSingle<T>(string pathToJson) where T : AbstractDefinition
        {
            if (string.IsNullOrEmpty(pathToJson))
                return null;

            var asset = Resources.Load<TextAsset>(pathToJson);            
            return Deserialize<T>(asset.text, asset.name);
        }

        public Dictionary<string, T> LoadCollection<T>(string pathToFolder) where T : AbstractDefinition
        {
            if (string.IsNullOrEmpty(pathToFolder))
                return null;

            var dic = new Dictionary<string, T>();
            var assets = Resources.LoadAll<TextAsset>(pathToFolder);

            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (asset == null)
                {
                    continue;
                }

                var definition = Deserialize<T>(asset.text, asset.name);
                if (definition == null)
                {
                    UnityEngine.Debug.LogWarning($"[Loader] Не удалось десериализовать деф '{asset.name}' из '{pathToFolder}'!");
                    continue;
                }

                // При дубликате id побеждает первый загруженный деф, остальные пропускаются
                if (!dic.TryAdd(definition.Id, definition))
                {
                    UnityEngine.Debug.LogWarning($"[Loader] Дубликат id '{definition.Id}' в '{pathToFolder}'! Деф пропущен.");
                    continue;
                }
            }

            return dic;
        }

        private T Deserialize<T>(string text, string name) where T : AbstractDefinition
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var definition = JsonConvert.DeserializeObject<T>(text);
            if (definition == null)
                return null;

            definition.Id = name;
            return definition;
        }
    }
}
