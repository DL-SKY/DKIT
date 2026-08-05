using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Read API параметров персонажа: сырые значения из <see cref="CharacterStateData.Parameters"/>
    /// и итоги по <see cref="RuleDef.ParameterFormulas"/>.
    /// </summary>
    /// <remarks>
    /// Конвенция (закреплена документацией; компилятором не enforced): читать через этот proxy
    /// (<see cref="CharacterParametersProxyBase.GetRawValue"/> / <see cref="CharacterParametersProxyBase.GetTotalValue"/>),
    /// а не индексировать <see cref="CharacterStateData.Parameters"/> напрямую.
    /// Мутации сырых параметров — у <see cref="CharacterParametersOperator"/> (Apply / Unapply
    /// <c>CharacterParamsPatchData</c>), не у этого класса.
    /// </remarks>
    public class CharacterParametersProxy : CharacterParametersProxyBase
    {
        private readonly CharacterStateData _characterState;

        public CharacterParametersProxy(
            CharacterStateData characterState,
            RuleDef ruleDef,
            DefinitionsManager definitionsManager)
            : base(ruleDef, definitionsManager)
        {
            _characterState = characterState;
        }

        protected override Dictionary<string, int> Parameters => _characterState?.Parameters;

        protected override string AncestryId => _characterState?.Ancestry;

        protected override string ClassId => _characterState?.Class;
    }
}
