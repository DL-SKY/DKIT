using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Definitions.Scripts.Examples
{
    public class Weapon
    {
        public int Damage;
        public int Durability;
    }

    public class ExampleWeapons
    {
        public Dictionary<string, Weapon> Weapons;
    }

    public class ExamplesLoader : MonoBehaviour
    {
        void Start()
        {
            var json = Resources.Load<TextAsset>("Examples/ExampleWeapons");
            UnityEngine.Debug.LogError($"json: {json}");

            var db = JsonConvert.DeserializeObject<ExampleWeapons>(json.ToString());
            UnityEngine.Debug.LogError($"db: {db.Weapons.Count}");
            foreach (var item in db.Weapons)
                UnityEngine.Debug.LogError($"        [ ] {item.Key}: {item.Value.Damage}/{item.Value.Durability}");



            // СЕРИАЛИЗАЦИЯ
            //
            //// Создаем объект для сериализации
            //var player = new PlayerData(
            //    "Hero",
            //    10,
            //    85.5f,
            //    new List<string> { "Sword", "Potion", "Key" }
            //);

            //// Сериализуем объект в JSON-строку
            //string json = JsonConvert.SerializeObject(player, Formatting.Indented);
            //Debug.Log(json);

            //// Сохраняем JSON в файл (в папку StreamingAssets)
            //string filePath = Path.Combine(Application.streamingAssetsPath, "playerData.json");
            //File.WriteAllText(filePath, json);
        }
    }
}
