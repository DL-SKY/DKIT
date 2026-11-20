using System;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Marker of the player's own hull. Icon comes from that hull def.
    /// The radial button list stays empty for now.
    /// </summary>
    public sealed class SpacePlayerMarkerViewModel : SpaceMarkerViewModel
    {
        public void Init(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
            Action<SpaceMarkerViewModel> onClick,
            Action<SpaceMarkerViewModel, string> onCommand)
        {
            Bind(snapshot, ReadHullIcon(snapshot, definitions), onClick, onCommand);
        }
    }
}
