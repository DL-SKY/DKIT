using System;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Marker of a sector target that is not a hull. Icon comes from the single marker-icon def.
    /// Speed is not shown. Radial buttons are the player's commands that allow this marker.
    /// </summary>
    public sealed class SpaceSectorMarkerViewModel : SpaceMarkerViewModel
    {
        public override bool ShowSpeed => false;

        public void Init(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
            SpaceProfile profile,
            Action<SpaceMarkerViewModel> onClick,
            Action<SpaceMarkerViewModel, string, string> onCommand)
        {
            Bind(snapshot, ReadKindIcon(snapshot, definitions), onClick, onCommand);
            AddPlayerCommands(definitions, profile);
        }

        private static string ReadKindIcon(SpaceMarkerSnapshot snapshot, SpaceDefinitionsManager definitions)
        {
            if (definitions?.MarkerSettings != null && definitions.MarkerSettings.TryGetIcon(snapshot.ObjectType, out var path))
                return path;

            UnityEngine.Debug.LogError($"[Space] Marker icons have no picture for {snapshot.ObjectType}.");
            return string.Empty;
        }
    }
}
