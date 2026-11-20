namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// One radial-menu button chosen by a marker view-model. The view draws it and does not add or drop buttons.
    /// </summary>
    public sealed class SpaceMarkerMenuButton
    {
        public SpaceMarkerMenuButton(string commandId, string sourceId, string label, string iconResourcesPath)
        {
            CommandId = commandId;
            SourceId = sourceId ?? string.Empty;
            Label = label;
            IconResourcesPath = iconResourcesPath ?? string.Empty;
        }

        public string CommandId { get; }
        public string SourceId { get; }
        public string Label { get; }
        public string IconResourcesPath { get; }
    }
}
