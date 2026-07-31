namespace Modules.State.Scripts.Actions.Models
{
    /// <summary>
    /// Category of state section(s) mutated by a state-action.
    /// Consumers filter by category, not by concrete action type.
    /// </summary>
    public enum StateChangeSource
    {
        Profile = 1,
        Wallet = 2,
        Localization = 3,
        Characters = 4,
        Inventory = 5,
        CharactersAndInventory = 6,
        AdventuresNavigation = 7,
        AdventuresParams = 8,
    }
}
