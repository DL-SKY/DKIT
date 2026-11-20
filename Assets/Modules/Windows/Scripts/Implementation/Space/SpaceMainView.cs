using System.Collections.Generic;
using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    public sealed class SpaceMainView : ViewBase<SpaceMainViewModel>
    {
        public static string Path = "Prefabs/Views/Space/SpaceMainView";

        [SerializeField] private RectTransform _markerHolder;

        private readonly Dictionary<string, SpaceMarkerView> _markers = new Dictionary<string, SpaceMarkerView>();
        private GameObject _markerPrefab;

        protected override void InitImplementation()
        {
            EnsureHolder();
            _markerPrefab = Resources.Load<GameObject>(SpaceMarkerView.Path);
            if (_markerPrefab == null)
                UnityEngine.Debug.LogError("[Space] Marker button prefab was not found at " + SpaceMarkerView.Path + ".");
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnMarkersChanged;
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnMarkersChanged;

            ClearMarkers();
        }

        private void LateUpdate()
        {
            if (_viewModel == null || _markerHolder == null)
                return;

            _viewModel.Tick();
            for (var i = 0; i < _viewModel.Markers.Count; i++)
            {
                var marker = _viewModel.Markers[i];
                if (marker == null || !_markers.TryGetValue(marker.TargetId, out var view) || view == null)
                    continue;

                view.Place(_markerHolder);
            }
        }

        private void OnMarkersChanged(string tag)
        {
            if (tag == SpaceMainViewModel.ON_MARKERS)
                SyncMarkers();
        }

        private void SyncMarkers()
        {
            if (_markerHolder == null || _markerPrefab == null || _viewModel == null)
                return;

            var alive = new HashSet<string>();
            for (var i = 0; i < _viewModel.Markers.Count; i++)
            {
                var marker = _viewModel.Markers[i];
                if (marker == null || string.IsNullOrEmpty(marker.TargetId))
                    continue;

                alive.Add(marker.TargetId);
                if (_markers.ContainsKey(marker.TargetId))
                    continue;

                var instance = Instantiate(_markerPrefab, _markerHolder);
                instance.name = marker.TargetId;
                var view = instance.GetComponent<SpaceMarkerView>();
                if (view == null)
                    view = instance.AddComponent<SpaceMarkerView>();

                view.Init(marker);
                _markers.Add(marker.TargetId, view);
            }

            var removed = new List<string>();
            foreach (var pair in _markers)
            {
                if (!alive.Contains(pair.Key))
                    removed.Add(pair.Key);
            }

            for (var i = 0; i < removed.Count; i++)
            {
                var id = removed[i];
                if (_markers.TryGetValue(id, out var view) && view != null)
                    Destroy(view.gameObject);

                _markers.Remove(id);
            }
        }

        private void EnsureHolder()
        {
            if (_markerHolder != null)
                return;

            var holderObject = new GameObject("Markers", typeof(RectTransform));
            holderObject.transform.SetParent(transform, false);
            _markerHolder = holderObject.GetComponent<RectTransform>();
            _markerHolder.anchorMin = Vector2.zero;
            _markerHolder.anchorMax = Vector2.one;
            _markerHolder.offsetMin = Vector2.zero;
            _markerHolder.offsetMax = Vector2.zero;
            _markerHolder.pivot = new Vector2(0.5f, 0.5f);
        }

        private void ClearMarkers()
        {
            foreach (var pair in _markers)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            }

            _markers.Clear();
        }
    }
}
