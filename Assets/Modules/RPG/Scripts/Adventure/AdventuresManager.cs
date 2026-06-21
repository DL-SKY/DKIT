namespace Modules.RPG.Scripts.Adventure
{
    public class AdventuresManager
    {
        private string _adventureId;

        public void Init(string adventureId)
        {
            UnityEngine.Debug.LogError($"AdventuresManager.Init({adventureId})");

            _adventureId = adventureId;
        }
    }
}
