namespace Modules.Windows.Scripts.Managers
{
    /// <summary>
    /// Queued toast-style hints. Place a MonoBehaviour implementation on the WindowsManager prefab.
    /// </summary>
    public interface IHintManager
    {
        void Show(string localizationKey);

        void Show(string localizationKey, float durationSeconds);

        void Clear();
    }
}
