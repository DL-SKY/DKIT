using System;
using System.IO;
using UnityEngine;

namespace Modules.Space.Scripts.Implementation.State
{
    /// <summary>
    /// Separate save location for the Space profile. The profile schema arrives with the data step.
    /// </summary>
    public sealed class SpaceStateManager
    {
        public const string SAVE_FOLDER = "Space";
        public const string PROFILE_FILE_NAME = "profile.json";

        public string ProfileDirectory { get; private set; }
        public string ProfileFilePath { get; private set; }
        public bool IsReady { get; private set; }

        public void Init()
        {
            if (string.IsNullOrEmpty(Application.persistentDataPath))
                throw new InvalidOperationException("Application.persistentDataPath is empty.");

            ProfileDirectory = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            Directory.CreateDirectory(ProfileDirectory);
            ProfileFilePath = Path.Combine(ProfileDirectory, PROFILE_FILE_NAME);
            IsReady = true;
        }
    }
}
