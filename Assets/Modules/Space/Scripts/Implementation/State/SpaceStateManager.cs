using System;
using System.Collections.Generic;
using System.IO;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.Space.Scripts.Implementation.State.Actions;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Interfaces;
using Modules.State.Scripts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.State
{
    /// <summary>
    /// Space profile on disk. <see cref="Init"/> prepares the folder. <see cref="LoadProfileState"/> reads the file.
    /// Player-progress writes go through state-actions. The flight ECS world does not store this profile.
    /// </summary>
    public sealed class SpaceStateManager : IStateManager<SpaceProfile>
    {
        public const string SAVE_FOLDER = "Space";
        public const string PROFILE_FILE_NAME = "profile.json";

        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.Indented
        };

        private const int SAVE_BATCH_SIZE = 2;

        private SpaceDefinitionsManager _definitions;
        private StateLogic<SpaceProfile> _logic;

        public string ProfileDirectory { get; private set; }
        public string ProfileFilePath { get; private set; }
        public bool IsReady { get; private set; }
        public SpaceProfile Profile { get; private set; }
        public SpaceProfile State => Profile;

        public void Init(SpaceDefinitionsManager definitions)
        {
            if (string.IsNullOrEmpty(Application.persistentDataPath))
                throw new InvalidOperationException("Application.persistentDataPath is empty.");

            _definitions = definitions;
            ProfileDirectory = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            Directory.CreateDirectory(ProfileDirectory);
            ProfileFilePath = Path.Combine(ProfileDirectory, PROFILE_FILE_NAME);
            _logic = new StateLogic<SpaceProfile>(this, Save, SAVE_BATCH_SIZE);
            Profile = null;
            IsReady = false;
        }

        public bool TrySetMaxCount(string queueId, int maxCount)
        {
            return Apply(new SetSpaceQueueMaxCountStateAction(queueId, maxCount), true);
        }

        public bool TryEnqueue(string queueId, string targetId, IReadOnlyList<SpaceTimeModifier> modifiers, long nowUnixMs)
        {
            return Apply(new EnqueueSpaceStateAction(_definitions, queueId, targetId, modifiers, nowUnixMs), true);
        }

        public bool TryInstallModule(string shipId, int slotIndex, string moduleId)
        {
            return Apply(new InstallSpaceModuleStateAction(_definitions, shipId, slotIndex, moduleId), true);
        }

        public bool TryRemoveModule(string shipId, int slotIndex)
        {
            return Apply(new RemoveSpaceModuleStateAction(shipId, slotIndex), true);
        }

        public void SaveProfileState(string profileId, bool useLightEncryption = false, string encryptionKey = null)
        {
            if (string.IsNullOrEmpty(ProfileDirectory))
                return;

            ProfileFilePath = Path.Combine(ProfileDirectory, FileNameOf(profileId));
            Save();
        }

        public SpaceProfile LoadProfileState(string profileId, bool useLightEncryption = false, string encryptionKey = null)
        {
            if (string.IsNullOrEmpty(ProfileDirectory))
                throw new InvalidOperationException("Space profile folder is not ready. Call Init first.");

            ProfileFilePath = Path.Combine(ProfileDirectory, FileNameOf(profileId));
            Profile = ReadOrCreate();
            IsReady = Apply(new EnsureSpaceQueuesStateAction(), false)
                && Apply(new EnsureSpaceShipStateAction(_definitions), true);
            return Profile;
        }

        public bool DeleteProfileState(string profileId)
        {
            if (string.IsNullOrEmpty(ProfileDirectory))
                return false;

            var path = Path.Combine(ProfileDirectory, FileNameOf(profileId));
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            if (string.Equals(path, ProfileFilePath, StringComparison.OrdinalIgnoreCase))
            {
                Profile = null;
                IsReady = false;
            }

            return true;
        }

        private bool Apply(IStateAction<SpaceProfile> action, bool forceSave)
        {
            if (_logic == null || Profile == null)
                return false;

            var result = _logic.ProcessAction(action, forceSave);
            if (result != null && result.IsValid)
                return true;

            UnityEngine.Debug.LogWarning("[Space] Profile action failed: " + (result != null ? result.ErrorMessage : "no result"));
            return false;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(ProfileFilePath) || Profile == null)
                return;

            var json = JsonConvert.SerializeObject(Profile, SETTINGS);
            File.WriteAllText(ProfileFilePath, json);
        }

        private SpaceProfile ReadOrCreate()
        {
            if (!File.Exists(ProfileFilePath))
                return new SpaceProfile();

            var json = File.ReadAllText(ProfileFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return new SpaceProfile();

            var profile = JsonConvert.DeserializeObject<SpaceProfile>(json, SETTINGS);
            return profile ?? new SpaceProfile();
        }

        private static string FileNameOf(string profileId)
        {
            if (string.IsNullOrEmpty(profileId))
                return PROFILE_FILE_NAME;

            var name = Path.GetFileName(profileId);
            return string.IsNullOrEmpty(name) ? PROFILE_FILE_NAME : name;
        }
    }
}
