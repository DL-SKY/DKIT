namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public enum CursorDefinitionSelectMode
    {
        Ancestry = 0,
        Class = 1,
        Background = 2,
    }

    public sealed class CursorDefinitionListItem
    {
        public string Id;
        public string Title;
        public string Description;
        public bool IsSelected;
    }
}
