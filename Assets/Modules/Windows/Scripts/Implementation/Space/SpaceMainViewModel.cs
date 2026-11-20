using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
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

        [Inject] private readonly ViewModelFactory _viewModelFactory;
        [Inject] private readonly SpaceSessionHost _sessionHost;
        [Inject] private readonly SpaceDefinitionsManager _definitions;

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
                    marker = _viewModelFactory.Create<SpaceMarkerViewModel>();
                    marker.Init(snapshot, _definitions, OnMarkerClick, OnMarkerCommand);
                    _markers.Add(marker);
                    changed = true;
                    continue;
                }

                marker.SetLive(snapshot.WorldPosition, snapshot.MetersPerSecond);
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
                _sessionHost.TrySetMoveTarget(marker.WorldPosition);
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
