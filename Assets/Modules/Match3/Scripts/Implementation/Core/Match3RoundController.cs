using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Match3.Scripts.Core;
using Modules.Windows.Scripts.Managers;
using Zenject;

namespace Modules.Match3.Scripts.Implementation.Core
{
    public class Match3RoundController : RoundControllerBase
    {
        private const string CELL_PREFAB = "Prefabs/Cells/Cell";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private GameZoneDef _def;


        public void Init(string defId)
        {
            UnityEngine.Debug.LogError($"Match3RoundController.Init({defId})");

            if (!_definitionsManager.GameZones.TryGetValue(defId, out var def))
            {
                UnityEngine.Debug.LogError($"[Match3RoundController.Init({defId})] Not found GameZones def with ID \"{defId}\"!");
                return;
            }

            _def = def;

            //TODO: ...
            InitBase(_def);
        }


        protected override void InitImplementation()
        {

        }

        protected override void Subscribe()
        {
            
        }

        protected override void Unsubscribe()
        {
            
        }

        


        public override void Dispose()
        {
            UnityEngine.Debug.LogError($"Match3RoundController.Dispose()");
        }
    }
}
