using Modules.Save.Scripts.Core;
using Modules.Save.Scripts.Interfaces;
using Modules.State.Scripts.Interfaces;
using System;

namespace Modules.State.Scripts.Core
{
    public abstract class StateManager<TStateData> : IStateManager<TStateData>
        where TStateData : class, IStateData, new()
    {
        private readonly ISaveManager _saveManager;
        public TStateData State { get; protected set; }

        protected StateManager()
        {
            _saveManager = new SaveManager();
        }

        protected StateManager(ISaveManager saveManager)
        {
            _saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));
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
                stateData = CreateNewState(profileId);

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

        protected abstract TStateData CreateNewState(string profileId);
    }
}
