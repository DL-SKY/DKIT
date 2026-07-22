namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    /// <summary>
    /// Stub for hub UI actions (create/select character, trade, party, adventure list).
    /// Window routing will be wired to WindowsManager / adventure presenters later.
    /// </summary>
    public class OpenWindowChoiceActionExecutor : IChoiceActionExecutor
    {
        private readonly string _windowId;

        public OpenWindowChoiceActionExecutor(string windowId)
        {
            _windowId = windowId;
        }

        public void Execute()
        {
            UnityEngine.Debug.LogWarning(
                $"[OpenWindowChoiceActionExecutor] OpenWindow is not wired yet. WindowId='{_windowId}'.");
        }
    }
}
