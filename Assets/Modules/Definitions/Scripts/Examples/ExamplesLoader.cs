using Modules.Definitions.Scripts.Core;
using Modules.Definitions.Scripts.Defs;
using Modules.Utils.Scripts.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Modules.Definitions.Scripts.Examples
{
    public class Weapon
    {
        public int Damage;
        public int Durability;
    }

    public class ExampleWeapons : AbstractDefinition
    {
        public Dictionary<string, Weapon> Weapons;
    }

    public class ExamplesLoader : MonoBehaviour
    {
        void Start()
        {
            //v1
            var json = Resources.Load<TextAsset>("Examples/ExampleWeapons");
            UnityEngine.Debug.LogError($"json: {json}");

            var db = JsonConvert.DeserializeObject<ExampleWeapons>(json.ToString());
            UnityEngine.Debug.LogError($"[vol1] db.count: {db.Weapons.Count}");
            foreach (var item in db.Weapons)
                UnityEngine.Debug.LogError($"        [v1] {item.Key}: {item.Value.Damage}/{item.Value.Durability}");


            UnityEngine.Debug.LogError($"  --------------------------------------  ");

            //v2
            var loader = new Loader();
            db = loader.LoadSingle<ExampleWeapons>("Examples/ExampleWeapons");
            UnityEngine.Debug.LogError($"[vol2: {db.Id}] db.count: {db.Weapons.Count}");
            foreach (var item in db.Weapons)
                UnityEngine.Debug.LogError($"        [v2] {item.Key}: {item.Value.Damage}/{item.Value.Durability}");




            //===========================================================================================



            // œ–»Ã≈– —Œ’–¿Õ≈Õ»ﬂ
            db.Weapons.Add("Shield", new Weapon { Damage = 0, Durability = 0 });
            // —ÂË‡ÎËÁÛÂÏ Ó·˙ÂÍÚ ‚ JSON-ÒÚÓÍÛ
            string save = JsonConvert.SerializeObject(db, Formatting.Indented);
            // —Óı‡ÌˇÂÏ JSON ‚ Ù‡ÈÎ (‚ Ô‡ÔÍÛ StreamingAssets)
            string filePath = Path.Combine(Application.persistentDataPath, $"{db.Id}.json");
            File.WriteAllText(filePath, save);

            UnityEngine.Debug.LogError($"File save in {Application.persistentDataPath}");




            // œ–»Ã≈– œ–Œ—“Œ√Œ ÿ»‘–Œ¬¿Õ»ﬂ / ƒ≈ÿ»‘–Œ¬¿Õ»ﬂ
            var key = "31415";
            var eSave = Crypto.XorEncryptDecrypt(save, key);
            filePath = Path.Combine(Application.persistentDataPath, $"{db.Id}_1.json");
            File.WriteAllText(filePath, eSave);

            eSave = Crypto.XorEncryptDecrypt(eSave, key);
            filePath = Path.Combine(Application.persistentDataPath, $"{db.Id}_2.json");
            File.WriteAllText(filePath, eSave);
        }
    }
}
