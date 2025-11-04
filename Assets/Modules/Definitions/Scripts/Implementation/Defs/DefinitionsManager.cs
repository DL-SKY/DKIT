using Modules.Definitions.Scripts.Core;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs
{
    public class DefinitionsManager
    {
        public Dictionary<string, GameZoneDef> GameZones;

        private readonly Loader _loader;


        public DefinitionsManager()
        {
            _loader = new Loader();
        }


        public void Init()
        {
            //...

            LoadAll();

            UnityEngine.Debug.LogError($"Init done! GameZones: {GameZones.Count}");

        }

        private void LoadAll()
        {
            LoadGameZones();
        }

        private void LoadGameZones()
        {
            GameZones = _loader.LoadCollection<GameZoneDef>("Definitions/GameZones");
        }
    }
}
