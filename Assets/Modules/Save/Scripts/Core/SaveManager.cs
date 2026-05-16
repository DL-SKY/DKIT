using Modules.Save.Scripts.Interfaces;
using Modules.Utils.Scripts.Security;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Modules.Save.Scripts.Core
{
    public class SaveManager : ISaveManager
    {
        private const string DEFAULT_FILE_EXTENSION = ".json";
        private const string DEFAULT_ENCRYPTION_KEY = "31415";

        public string SavesDirectory { get; }

        public SaveManager()
        {
            SavesDirectory = Path.Combine(Application.persistentDataPath, "saves");
            Directory.CreateDirectory(SavesDirectory);
        }

        public void Save<T>(string profileId, T data, bool useLightEncryption = false, string encryptionKey = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(profileId))
                throw new ArgumentException("Profile id is null or empty.", nameof(profileId));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var path = GetProfilePath(profileId);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var payload = useLightEncryption
                ? Crypto.XorEncryptDecrypt(json, ResolveEncryptionKey(encryptionKey))
                : json;

            File.WriteAllText(path, payload);
        }

        public bool TryLoad<T>(string profileId, out T data, bool useLightEncryption = false, string encryptionKey = null) where T : class
        {
            data = null;

            if (string.IsNullOrWhiteSpace(profileId))
            {
                UnityEngine.Debug.LogWarning("[SaveManager] TryLoad failed: profile id is null or empty.");
                return false;
            }

            var path = GetProfilePath(profileId);
            if (!File.Exists(path))
                return false;

            var payload = File.ReadAllText(path);
            if (string.IsNullOrEmpty(payload))
                return false;

            try
            {
                var json = useLightEncryption
                    ? Crypto.XorEncryptDecrypt(payload, ResolveEncryptionKey(encryptionKey))
                    : payload;

                data = JsonConvert.DeserializeObject<T>(json);
                return data != null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[SaveManager] TryLoad failed for profile '{profileId}': {ex.Message}");
                return false;
            }
        }

        public bool Delete(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return false;

            var path = GetProfilePath(profileId);
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }

        private string GetProfilePath(string profileId)
        {
            var safeProfileId = profileId.Trim();
            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
                safeProfileId = safeProfileId.Replace(invalidFileNameChar, '_');

            return Path.Combine(SavesDirectory, $"{safeProfileId}{DEFAULT_FILE_EXTENSION}");
        }

        private static string ResolveEncryptionKey(string encryptionKey)
        {
            return string.IsNullOrEmpty(encryptionKey) ? DEFAULT_ENCRYPTION_KEY : encryptionKey;
        }
    }
}
