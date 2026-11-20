namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// One radial-menu button chosen by a marker view-model. The view draws it and does not add or drop buttons.
    /// </summary>
    public sealed class SpaceMarkerMenuButton
    {
        public SpaceMarkerMenuButton(string commandId, string label, string iconResourcesPath)
        {
            CommandId = commandId;
            Label = label;
            IconResourcesPath = iconResourcesPath ?? string.Empty;
        }

        public string CommandId { get; }
        public string Label { get; }
        public string IconResourcesPath { get; }
    }
}
