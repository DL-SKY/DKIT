namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Входные данные для <c>UpdateCharacterStateAction</c>.
    /// </summary>
    /// <remarks>
    /// Контракт: только прокачка персонажа (достижение нового уровня), не произвольное редактирование.
    /// UI прокачки делает слепок персонажа, затем последовательно применяет feats нового уровня
    /// (включая выбор фитов / какие параметры качать) через тот же путь Apply / Unapply,
    /// что и остальные патчи параметров. Итоговый mutable-блок уходит в <see cref="CharacterData"/>.
    /// </remarks>
    public class UpdateCharacterRequestData
    {
        public int CharacterId;
        public CharacterRequestData CharacterData;
    }
}
