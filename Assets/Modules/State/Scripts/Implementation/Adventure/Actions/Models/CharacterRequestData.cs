using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Переиспользуемый mutable-блок персонажа для create/update state-actions.
    /// </summary>
    /// <remarks>
    /// При update (прокачка) <see cref="Parameters"/> собирается через Write API
    /// (<see cref="CharacterParametersOperator"/> / методы Apply* / Unapply* ниже),
    /// а не произвольными правками словаря вне этого пути.
    /// </remarks>
    public class CharacterRequestData
    {
        public Dictionary<string, int> Parameters;
        public List<EquippedItemStateData> EquippedItems;
        public Dictionary<string, int> Spells;
        public Dictionary<string, int> StatusEffects;

        public void ApplyFeat(string featId, DefinitionsManager definitionsManager)
        {
            EnsureParameters();
            CharacterParametersOperator.ApplyFeat(Parameters, featId, definitionsManager);
        }

        public void UnapplyFeat(string featId, DefinitionsManager definitionsManager)
        {
            EnsureParameters();
            CharacterParametersOperator.UnapplyFeat(Parameters, featId, definitionsManager);
        }

        public void ApplyPatch(CharacterParamsPatchData patch, DefinitionsManager definitionsManager)
        {
            EnsureParameters();
            CharacterParametersOperator.ApplyPatch(Parameters, patch, definitionsManager);
        }

        public void UnapplyPatch(CharacterParamsPatchData patch, DefinitionsManager definitionsManager)
        {
            EnsureParameters();
            CharacterParametersOperator.UnapplyPatch(Parameters, patch, definitionsManager);
        }

        private void EnsureParameters()
        {
            Parameters ??= new Dictionary<string, int>();
        }
    }
}
