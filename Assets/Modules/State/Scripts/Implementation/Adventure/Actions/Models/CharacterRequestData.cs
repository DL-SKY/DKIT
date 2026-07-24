using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Reusable character mutable block for create/update state-actions.
    /// </summary>
    public class CharacterRequestData
    {
        public Dictionary<string, int> Parameters;
        public List<EquippedItemStateData> EquippedItems;
        public Dictionary<string, int> Spells;
        public Dictionary<string, int> StatusEffects;
    }
}
