using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Settings;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Space
{
    public sealed class SpaceMainViewModel : ViewModelBase
    {
        public const string ON_MARKERS = "markers";
        private const string APPROACH_COMMAND_ID = "approach";
        private const string STOP_COMMAND_ID = "stop";

        [Inject] private readonly ViewModelFactory _viewModelFactory;
        [Inject] private readonly SpaceSessionHost _sessionHost;
        [Inject] private readonly SpaceDefinitionsManager _definitions;
        [Inject] private readonly SpaceStateManager _state;

        private readonly List<SpaceMarkerViewModel> _markers = new List<SpaceMarkerViewModel>();
        private readonly List<SpaceMarkerSnapshot> _snapshots = new List<SpaceMarkerSnapshot>();
        private SpaceMarkerViewModel _openMarker;

        public IReadOnlyList<SpaceMarkerViewModel> Markers => _markers;

        public void Init()
        {
        }

        public void Tick()
        {
            if (_sessionHost == null)
                return;

            _sessionHost.CopyMarkers(_snapshots);
            var changed = false;
            for (var i = _markers.Count - 1; i >= 0; i--)
            {
                if (FindSnapshot(_markers[i].TargetId) >= 0)
                    continue;

                if (_openMarker == _markers[i])
                    _openMarker = null;

                _markers[i].Dispose();
                _markers.RemoveAt(i);
                changed = true;
            }

            for (var i = 0; i < _snapshots.Count; i++)
            {
                var snapshot = _snapshots[i];
                var marker = FindMarker(snapshot.TargetId);
                if (marker == null)
                {
                    marker = CreateMarker(snapshot);
                    _markers.Add(marker);
                    changed = true;
                    continue;
                }

                marker.SetLive(snapshot.WorldPosition, snapshot.MetersPerSecond, snapshot.MaxMetersPerSecond);
            }

            if (changed)
                SendOnChange(ON_MARKERS);
        }

        protected override Options CreateOptions()
        {
            return new Options(canCloseOnEsc: false, hideInHistory: false, sortingLayer: SortingOrderLayer.COMMON);
        }

        public override void Dispose()
        {
            for (var i = 0; i < _markers.Count; i++)
                _markers[i].Dispose();

            _markers.Clear();
            _openMarker = null;
        }

        private void OnMarkerClick(SpaceMarkerViewModel marker)
        {
            if (marker == null)
                return;

            if (_openMarker == marker)
            {
                marker.SetMenuOpen(false);
                _openMarker = null;
                return;
            }

            if (_openMarker != null)
                _openMarker.SetMenuOpen(false);

            marker.SetMenuOpen(true);
            _openMarker = marker;
        }

        private void OnMarkerCommand(SpaceMarkerViewModel marker, string commandId)
        {
            if (marker != null && commandId == APPROACH_COMMAND_ID)
                _sessionHost.TrySetMoveTarget(marker.TargetId);
            else if (commandId == STOP_COMMAND_ID)
                _sessionHost.TryStopPlayer();
            else
                UnityEngine.Debug.Log($"[Space] Marker '{marker?.DefId}' command '{commandId}'.");

            if (marker != null)
                marker.SetMenuOpen(false);

            if (_openMarker == marker)
                _openMarker = null;
        }

        private int FindSnapshot(string targetId)
        {
            for (var i = 0; i < _snapshots.Count; i++)
            {
                if (_snapshots[i].TargetId == targetId)
                    return i;
            }

            return -1;
        }

        private SpaceMarkerViewModel CreateMarker(SpaceMarkerSnapshot snapshot)
        {
            if (snapshot.Player)
            {
                var player = _viewModelFactory.Create<SpacePlayerMarkerViewModel>();
                player.Init(snapshot, _definitions, _state != null ? _state.Profile : null, OnMarkerClick, OnMarkerCommand);
                return player;
            }

            if (snapshot.Ship)
            {
                var ship = _viewModelFactory.Create<SpaceShipMarkerViewModel>();
                ship.Init(snapshot, _definitions, _state != null ? _state.Profile : null, OnMarkerClick, OnMarkerCommand);
                return ship;
            }

            var sector = _viewModelFactory.Create<SpaceSectorMarkerViewModel>();
            sector.Init(snapshot, _definitions, _state != null ? _state.Profile : null, OnMarkerClick, OnMarkerCommand);
            return sector;
        }

        private SpaceMarkerViewModel FindMarker(string targetId)
        {
            for (var i = 0; i < _markers.Count; i++)
            {
                if (_markers[i].TargetId == targetId)
                    return _markers[i];
            }

            return null;
        }
    }
}
