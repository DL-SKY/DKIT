using Modules.Save.Scripts.Core;
using Modules.Save.Scripts.Interfaces;
using Modules.State.Scripts.Interfaces;
using System;
using UnityEngine;

namespace Modules.State.Scripts.Core
{
    public abstract class StateManager<TStateData> : IStateManager<TStateData>
        where TStateData : class, IStateData, new()
    {
        private ISaveManager _saveManager;
        private bool _isSubscribedToApplicationQuitting;
        public TStateData State { get; protected set; }


        public void Init(string folder, string extension, string key)
        {
            _saveManager = new SaveManager(folder, extension, key);
            SubscribeToApplicationQuitting();
        }

        public void Init(ISaveManager saveManager)
        {
            _saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));
            SubscribeToApplicationQuitting();
        }


        public virtual void SaveProfileState(string profileId, bool useLightEncryption = false, string encryptionKey = null)
        {
            if (State == null)
                State = CreateNewState(profileId);

            _saveManager.Save(profileId, State, useLightEncryption, encryptionKey);
        }

        public TStateData LoadProfileState(string profileId, bool useLightEncryption = false, string encryptionKey = null)
        {
            if (!_saveManager.TryLoad(profileId, out TStateData stateData, useLightEncryption, encryptionKey))
            {
                stateData = CreateNewState(profileId);
                SaveProfileState(profileId, useLightEncryption, encryptionKey);
            }

            State = stateData;
            return State;
        }

        public bool DeleteProfileState(string profileId)
        {
            var deleted = _saveManager.Delete(profileId);
            if (deleted)
                State = null;

            return deleted;
        }

        private void SubscribeToApplicationQuitting()
        {
            if (_isSubscribedToApplicationQuitting)
                return;

            Application.quitting += HandleApplicationQuitting;
            _isSubscribedToApplicationQuitting = true;
        }

        private void HandleApplicationQuitting()
        {
            OnApplicationQuitting();
        }

        protected abstract void OnApplicationQuitting();
        protected abstract TStateData CreateNewState(string profileId);
    }
}
