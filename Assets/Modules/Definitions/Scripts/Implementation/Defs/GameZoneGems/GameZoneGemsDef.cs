using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Defs.GameZoneGems
{
    public class GameZoneGemsDef : AbstractDefinition
    {
        /// <summary>
        /// Обязательные гемы, которые будут в раунде
        /// Key - defId; Value - weight for randomize
        /// </summary>
        public Dictionary<string, int> Gems;

        /// <summary>
        /// (Опционально)
        /// Число дополнительных гемов, которые будут сгенерированы из коллекции AdditionalGems
        /// </summary>
        public int AdditionalGemsCount;

        /// <summary>
        /// (Опционально)
        /// Дополнительные гемы со своими весакми для рандома уровня, если требуется
        /// Key - KeyValuePair<defId, weight>; Value - weight for additional randomize
        /// </summary>
        public Dictionary<KeyValuePair<string, int>, int> AdditionalGems;
    }
}
