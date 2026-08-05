using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Read API параметров создаваемого персонажа: сырые значения из
    /// <see cref="CharacterRequestData.Parameters"/> и итоги по <see cref="RuleDef.ParameterFormulas"/>.
    /// </summary>
    /// <remarks>
    /// Ancestry / Class берутся с <see cref="CreateCharacterRequestData"/>;
    /// словарь параметров — из <see cref="CreateCharacterRequestData.CharacterData"/>.
    /// </remarks>
    public class CreatedCharacterParametersProxy : CharacterParametersProxyBase
    {
        private readonly CreateCharacterRequestData _request;

        public CreatedCharacterParametersProxy(
            CreateCharacterRequestData request,
            RuleDef ruleDef,
            DefinitionsManager definitionsManager)
            : base(ruleDef, definitionsManager)
        {
            _request = request;
        }

        protected override Dictionary<string, int> Parameters => _request?.CharacterData?.Parameters;

        protected override string AncestryId => _request?.Ancestry;

        protected override string ClassId => _request?.Class;
    }
}
