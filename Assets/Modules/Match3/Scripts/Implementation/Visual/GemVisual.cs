using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using UnityEngine;

namespace Modules.Match3.Scripts.Implementation.Visual
{
    public class GemVisual : MonoBehaviour
    {
        private GemDef _def;

        public void Init(GemDef def)
        {
            _def = def;
        }

        public void StartDestroyAnimation()
        {
            //UnityEngine.Debug.LogError($"    [GemVisual] {name} :: StartDestroyAnimation()");
            //TODO: ...
        }

        private void OnDestroy()
        {
            //UnityEngine.Debug.LogError($"    [GemVisual] {name} :: OnDestroy()");
            //TODO: ...
        }
    }
}
