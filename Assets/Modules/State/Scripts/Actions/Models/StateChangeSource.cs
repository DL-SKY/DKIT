namespace Modules.State.Scripts.Actions.Models
{
    public enum StateChangeSource
    {
        ChangeWalletResource = 2,
        SetWalletResource = 3,

        SetProfileUpdateTime = 4,

        SetCurrentAdventureId = 5,
        SetCurrentAdventureSceneId = 6,

        SetLocalizationLanguage = 7,

        SetWorldParams = 8,
        SetAdventureParams = 9,
        SetGlobalParams = 10,
        CreateCharacter = 11,
        UpdateCharacter = 12,

        EquipItemFromInventory = 13,
        UnequipItemToInventory = 14,
        MoveEquippedItemBetweenSlots = 15,

        AddInventoryItems = 16,
        RemoveInventoryItems = 17,
        AddCharacterItem = 18,
        RemoveCharacterEquippedItem = 19,
        EndCharacterTurn = 20,
    }
}
