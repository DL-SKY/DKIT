using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Implementation.Adventures.Defs
{
    /// <summary>
    /// Патч числовых параметров персонажа для записи в
    /// <see cref="Modules.State.Scripts.Implementation.Adventure.StateDatas.CharacterStateData.Parameters"/>.
    /// В дефе только данные; семантика Apply / Unapply принадлежит Write API параметров персонажа (модуль State).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Apply</b>
    /// <list type="bullet">
    /// <item><see cref="Add"/> — для каждого ключа прибавить значение к текущему сырому параметру (<c>+=</c>).</item>
    /// <item><see cref="Set"/> — для каждого ключа присвоить значение (<c>=</c>). Bool-флаги: <c>0</c> / ненулевое.</item>
    /// <item><see cref="AlsoApplyFeatIds"/> — каскадный Apply связанных <see cref="Feats.FeatDef"/> (те же правила).</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Unapply</b> (обратная операция к ранее применённому патчу)
    /// <list type="bullet">
    /// <item><see cref="Add"/> — для каждого ключа прибавить отрицание значения (<c>+= -value</c>).</item>
    /// <item><see cref="Set"/> — инверсия флага: если патч ставил <b>ненулевое</b> значение — записать <c>0</c>;
    /// если патч ставил <c>0</c> — записать <c>1</c>. Предыдущее произвольное целое не восстанавливается.</item>
    /// <item><see cref="AlsoApplyFeatIds"/> — каскадный Unapply связанных feat id (обычно в обратном порядке).</item>
    /// </list>
    /// </para>
    /// <para>
    /// Итоги вроде <c>MaxHitPoints</c> / навыков сюда не пишутся — храните сырые ключи
    /// (например <c>MaxHitPoints.Bonus</c>) и читайте через <c>CharacterParametersProxy.GetTotalValue</c>.
    /// </para>
    /// </remarks>
    public class CharacterParamsPatchData
    {
        /// <summary>
        /// Ключ параметра → прибавить к текущему сырому значению (характеристики, бонусы, модификаторы).
        /// Ключи согласованы с <see cref="Constants.Glossary.Characters"/> и согласованными id черт.
        /// </summary>
        public Dictionary<string, int> Add;

        /// <summary>
        /// Ключ параметра → установить значение (proficiency, флаги черты: <c>0</c> — false, иначе true).
        /// При Unapply: ненулевое становится <c>0</c>, <c>0</c> становится <c>1</c>.
        /// </summary>
        public Dictionary<string, int> Set;

        /// <summary>
        /// Id других <see cref="Feats.FeatDef"/> для каскадного Apply / Unapply
        /// (цепочки background → feat и т.п.).
        /// </summary>
        public List<string> AlsoApplyFeatIds;
    }
}
