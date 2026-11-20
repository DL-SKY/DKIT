using System;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Marker of a sector target that is not a hull. Icon comes from the single marker-icon def.
    /// Speed is not shown. The radial button list stays empty for now.
    /// </summary>
    public sealed class SpaceSectorMarkerViewModel : SpaceMarkerViewModel
    {
        public override bool ShowSpeed => false;

        public void Init(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
            Action<SpaceMarkerViewModel> onClick,
            Action<SpaceMarkerViewModel, string> onCommand)
        {
            Bind(snapshot, ReadKindIcon(snapshot, definitions), onClick, onCommand);
        }

        private static string ReadKindIcon(SpaceMarkerSnapshot snapshot, SpaceDefinitionsManager definitions)
        {
            if (definitions?.MarkerIcons != null && definitions.MarkerIcons.TryGetIcon(snapshot.Kind, out var path))
                return path;

            UnityEngine.Debug.LogError($"[Space] Marker icons have no picture for {snapshot.Kind}.");
            return string.Empty;
        }
    }
}
