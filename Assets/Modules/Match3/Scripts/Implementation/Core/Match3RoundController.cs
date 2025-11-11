using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Match3.Scripts.Core;
using Modules.Windows.Scripts.Managers;
using Zenject;

namespace Modules.Match3.Scripts.Implementation.Core
{
    public class Match3RoundController : RoundControllerBase
    {
        [Inject] private readonly WindowsManager _windowsManager;

        private GameZoneDef _def;


        public void Init(GameZoneDef def)
        {
            _def = def;

            //TODO: ...
            InitBase(def);
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
