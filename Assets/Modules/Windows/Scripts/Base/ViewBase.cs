using Modules.Windows.Scripts.Settings;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Base
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class ViewBase<T> : MonoBehaviour, IView where T : ViewModelBase
    {
        public event Action<int> OnViewDestroy;

        public int Handle { get; private set; }
        public Options Options => _viewModel.Options;

        protected T _viewModel;

        private bool _isInited;
        private Canvas _canvas;

        protected virtual void Awake()
        {
            Handle = GenerateHandle();
            _canvas = GetComponent<Canvas>();
        }

        private int GenerateHandle()
        {
            return string.Format("{0}-{1}", typeof(T), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).GetHashCode();
        }

        public void Init(T viewModel)
        {
            _viewModel = viewModel;
            _viewModel.SetHandle(Handle);

            Subscribe();

            _isInited = true;
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();

        public void SetSortingOrder(int value)
        {
            _canvas.sortingOrder = value;
        }

        public virtual void Show()
        {

        }

        public virtual void Hide()
        {
            GameObject.Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            Unsubscribe();

            _viewModel.Dispose();
            OnViewDestroy?.Invoke(Handle);
        }
    }
}
