using System;
using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// One screen marker. The picture and the click menu come from the tracked def.
    /// </summary>
    public sealed class SpaceMarkerViewModel : ViewModelBase
    {
        public const string ON_MENU = "menu";

        private readonly List<string> _commands = new List<string>();
        private Action<SpaceMarkerViewModel> _onClick;
        private Action<SpaceMarkerViewModel, string> _onCommand;
        private bool _menuOpen;

        public string TargetId { get; private set; }
        public string DefId { get; private set; }
        public bool Player { get; private set; }
        public SpaceObjectKind Kind { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public string IconResourcesPath { get; private set; }
        public bool MenuOpen => _menuOpen;
        public IReadOnlyList<string> Commands => _commands;

        public void Init(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
            Action<SpaceMarkerViewModel> onClick,
            Action<SpaceMarkerViewModel, string> onCommand)
        {
            TargetId = snapshot.TargetId;
            DefId = snapshot.DefId;
            Player = snapshot.Player;
            Kind = snapshot.Kind;
            WorldPosition = snapshot.WorldPosition;
            IconResourcesPath = ResolveIconPath(snapshot);
            _onClick = onClick;
            _onCommand = onCommand;
            _commands.Clear();
            CollectCommands(snapshot, definitions);
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
        }

        public void SetMenuOpen(bool open)
        {
            if (_menuOpen == open)
                return;

            _menuOpen = open;
            SendOnChange(ON_MENU);
        }

        public void Click()
        {
            _onClick?.Invoke(this);
        }

        public void ClickCommand(string commandId)
        {
            _onCommand?.Invoke(this, commandId);
        }

        public override void Dispose()
        {
            _onClick = null;
            _onCommand = null;
            _commands.Clear();
            _menuOpen = false;
        }

        private void CollectCommands(SpaceMarkerSnapshot snapshot, SpaceDefinitionsManager definitions)
        {
            if (definitions == null || string.IsNullOrEmpty(snapshot.DefId))
                return;

            if (snapshot.Player)
            {
                if (definitions.TryGetHull(snapshot.DefId, out var hull))
                    CopyCommands(hull?.CommandIds);
                return;
            }

            if (definitions.TryGetObject(snapshot.DefId, out var body))
                CopyCommands(body?.CommandIds);
        }

        private void CopyCommands(List<string> commandIds)
        {
            if (commandIds == null)
                return;

            for (var i = 0; i < commandIds.Count; i++)
            {
                var commandId = commandIds[i];
                if (string.IsNullOrEmpty(commandId) || _commands.Contains(commandId))
                    continue;

                _commands.Add(commandId);
            }
        }

        private static string ResolveIconPath(SpaceMarkerSnapshot snapshot)
        {
            if (snapshot.Player)
                return "Sprites/Space/Markers/Player";

            switch (snapshot.Kind)
            {
                case SpaceObjectKind.Asteroid:
                    return "Sprites/Space/Markers/Asteroid";
                case SpaceObjectKind.Station:
                    return "Sprites/Space/Markers/Station";
                default:
                    return "Sprites/Space/Markers/Point";
            }
        }
    }
}
