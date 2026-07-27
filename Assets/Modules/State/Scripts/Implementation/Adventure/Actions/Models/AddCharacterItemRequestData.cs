namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Выдача одного предмета персонажу: сначала свободный Bag-слот, иначе общий инвентарь.
    /// </summary>
    public class AddCharacterItemRequestData
    {
        public int CharacterId;
        public string ItemId;
    }
}
