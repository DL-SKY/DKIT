namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// One radial-menu button chosen by a marker view-model. The view draws it and does not add or drop buttons.
    /// </summary>
    public sealed class SpaceMarkerMenuButton
    {
        public SpaceMarkerMenuButton(string commandId, string label)
        {
            CommandId = commandId;
            Label = label;
        }

        public string CommandId { get; }
        public string Label { get; }
    }
}
