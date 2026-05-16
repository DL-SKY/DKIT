using Modules.Save.Scripts.Core;
using Modules.Save.Scripts.Data;
using UnityEngine;

namespace Modules.Save.Examples
{
    public class SaveManagerExample : MonoBehaviour
    {
        private void Start()
        {
            var saveManager = new SaveManager();
            var profileId = "example_profile";

            var state = new StateData
            {
                Id = 1,
                Player = new PlayerData(),
                Adventure = new AdventureStateData()
            };

            saveManager.Save(profileId, state, useLightEncryption: true);

            if (saveManager.TryLoad(profileId, out StateData loadedState, useLightEncryption: true))
            {
                UnityEngine.Debug.Log($"[SaveManagerExample] Loaded state id: {loadedState.Id}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[SaveManagerExample] Failed to load state.");
            }

            var deleted = saveManager.Delete(profileId);
            UnityEngine.Debug.Log($"[SaveManagerExample] Save deleted: {deleted}");
        }
    }
}
