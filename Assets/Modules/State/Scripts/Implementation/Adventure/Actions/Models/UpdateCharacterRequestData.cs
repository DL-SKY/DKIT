namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Input payload for updating an existing character mutable block.
    /// </summary>
    public class UpdateCharacterRequestData
    {
        public int CharacterId;
        public CharacterRequestData CharacterData;
    }
}
