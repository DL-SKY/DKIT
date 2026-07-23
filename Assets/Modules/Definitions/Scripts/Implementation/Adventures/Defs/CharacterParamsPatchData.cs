using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs
{
    /// <summary>
    /// Патч числовых параметров персонажа для записи в <see cref="Modules.State.Scripts.Implementation.Adventure.StateDatas.CharacterStateData.Parameters"/>.
    /// Семантика применения (сложение, установка, цепочка feat) — на стороне runtime; в дефе только данные.
    /// </summary>
    public class CharacterParamsPatchData
    {
        /// <summary>
        /// Ключ параметра → прибавить к текущему значению (характеристики, бонусы, модификаторы).
        /// Ключи согласуются с <see cref="Constants.Glossary.Characters"/> и согласованными id черт.
        /// </summary>
        public Dictionary<string, int> Add;

        /// <summary>
        /// Ключ параметра → установить значение (proficiency, флаг владения чертой: 0 — false, иначе true).
        /// </summary>
        public Dictionary<string, int> Set;

        /// <summary>
        /// Id других <see cref="Feats.FeatDef"/> для каскадного применения (цепочки background → feat и т.п.).
        /// </summary>
        public List<string> AlsoApplyFeatIds;
    }
}
