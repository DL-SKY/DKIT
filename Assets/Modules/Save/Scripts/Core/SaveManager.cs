using Modules.Save.Scripts.Interfaces;
using Modules.Utils.Scripts.Security;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Modules.Save.Scripts.Core
{
    public sealed class SaveManager : ISaveManager
    {
        public const string DEFAULT_FILE_FOLDER = "Saves";
        public const string DEFAULT_FILE_EXTENSION = ".json";
        public const string DEFAULT_ENCRYPTION_KEY = "31415";

        public string SavesDirectory { get; private set; }
        public string FileExtension { get; private set; }
        public string EncryptionKey { get; private set; }

        public SaveManager()
        {
            Init(DEFAULT_FILE_FOLDER, DEFAULT_FILE_EXTENSION, DEFAULT_ENCRYPTION_KEY);
        }

        public SaveManager(string folder, string extension, string key)
        {
            Init(folder, extension, key);
        }

        private void Init(string folder, string extension, string key)
        {
            SavesDirectory = Path.Combine(Application.persistentDataPath, folder);
            FileExtension = extension;
            EncryptionKey = key;

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

            UnityEngine.Debug.LogError($" >> GetProfilePath({profileId}) :: {Path.Combine(SavesDirectory, $"{safeProfileId}{FileExtension}")}");
            return Path.Combine(SavesDirectory, $"{safeProfileId}{FileExtension}");
        }

        private string ResolveEncryptionKey(string encryptionKey)
        {
            return string.IsNullOrEmpty(encryptionKey) ? EncryptionKey : encryptionKey;
        }
    }
}
