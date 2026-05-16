namespace Modules.State.Scripts.Interfaces
{
    public interface IStateManager<TStateData> where TStateData : class, IStateData, new()
    {
        TStateData State { get; }

        void SaveProfileState(string profileId, bool useLightEncryption = false, string encryptionKey = null);

        TStateData LoadProfileState(string profileId, bool useLightEncryption = false, string encryptionKey = null);

        bool DeleteProfileState(string profileId);
    }
}
