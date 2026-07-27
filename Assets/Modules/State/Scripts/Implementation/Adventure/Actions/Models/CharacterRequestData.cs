using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Переиспользуемый mutable-блок персонажа для create/update state-actions.
    /// </summary>
    /// <remarks>
    /// При update (прокачка) <see cref="Parameters"/> ожидается собранным через Write API
    /// параметров персонажа (Apply / Unapply <c>CharacterParamsPatchData</c>),
    /// а не произвольными правками словаря вне этого пути.
    /// </remarks>
    public class CharacterRequestData
    {
        public Dictionary<string, int> Parameters;
        public List<EquippedItemStateData> EquippedItems;
        public Dictionary<string, int> Spells;
        public Dictionary<string, int> StatusEffects;
    }
}
