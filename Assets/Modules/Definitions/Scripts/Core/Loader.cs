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
            foreach (var asset in assets)
                dic.Add(asset.name, Deserialize<T>(asset.text, asset.name));

            return dic;
        }

        private T Deserialize<T>(string text, string name) where T : AbstractDefinition
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var definition = JsonConvert.DeserializeObject<T>(text);
            definition.Id = name;
            return definition;
        }
    }
}
