using Modules.Windows.Scripts.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Managers
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public sealed class WindowsManager : MonoBehaviour
    {
        [SerializeField] private Transform _viewsHolder;

        private Dictionary<int, IView> _views = new Dictionary<int, IView>();
        private List<int> _history = new List<int>();

        private void Update()
        {
            //Back button
            if (Input.GetKeyDown(KeyCode.Escape))
                TryCloseCurrentView();
        }

        public V OpenView<V, M>(string viewPath, M model) where V: ViewBase<M> where M : ViewModelBase
        {
            var prefab = Resources.Load<V>(viewPath);
            var instance = Instantiate<V>(prefab, _viewsHolder);
            instance.Init(model);
            instance.OnViewDestroy += OnViewDestroyHandler;            

            if (!model.Options.HideInHistory)
                _history.Add(instance.Handle);
            _views.Add(instance.Handle, instance);

            instance.SetSortingOrder(_views.Count + (int)instance.Options.SortingLayer);
            instance.Show();

            UnityEngine.Debug.Log($"[WindowsManager] Open window {instance.GetType().Name}({instance.Handle} / 0x{instance.Handle:X8}).");

            return instance;
        }

        public void CloseView(int handle)
        {
            if (_views.TryGetValue(handle, out var view))
            {
                view.Hide();

                UnityEngine.Debug.Log($"[WindowsManager] Close window {view.GetType().Name}({view.Handle} / 0x{view.Handle:X8}).");
            }
        }

        private void OnViewDestroyHandler(int handle)
        {
            if (_views.TryGetValue(handle, out var view))
                view.OnViewDestroy -= OnViewDestroyHandler;

            _views.Remove(handle);
            _history.Remove(handle);
        }

        private void TryCloseCurrentView()
        {
            if (_history.Count > 0)
                if (_views.TryGetValue(_history[^1], out var view))
                    if (view.Options.CanCloseOnEsc)
                        CloseView(view.Handle);
        }

        private void OnDestroy()
        {
            foreach (var view in _views)
                if (view.Value != null)
                    view.Value.OnViewDestroy -= OnViewDestroyHandler;

            _views.Clear();
            _history.Clear();
        }
    }
}
