namespace Modules.Save.Scripts.Interfaces
{
    public interface ISaveManager
    {
        string SavesDirectory { get; }

        void Save<T>(string profileId, T data, bool useLightEncryption = false, string encryptionKey = null) where T : class;

        bool TryLoad<T>(string profileId, out T data, bool useLightEncryption = false, string encryptionKey = null) where T : class;

        bool Delete(string profileId);
    }
}
