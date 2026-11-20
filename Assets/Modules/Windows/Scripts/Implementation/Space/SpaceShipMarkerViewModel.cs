using System;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Marker of a hull that is not the player's ship. Icon comes from that hull def.
    /// Radial buttons are the player's commands that allow this marker.
    /// </summary>
    public sealed class SpaceShipMarkerViewModel : SpaceMarkerViewModel
    {
        public void Init(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
            SpaceProfile profile,
            Action<SpaceMarkerViewModel> onClick,
            Action<SpaceMarkerViewModel, string> onCommand)
        {
            Bind(snapshot, ReadHullIcon(snapshot, definitions), onClick, onCommand);
            AddPlayerCommands(definitions, profile);
        }
    }
}
