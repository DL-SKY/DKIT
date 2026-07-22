using Modules.RPG.Scripts.Adventure.Data;
using Modules.Windows.Scripts.Base;
using System;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items
{
    /// <summary>
    /// Base ViewModel for a single adventure scene content item.
    /// Created via Zenject; call <see cref="Init"/> before use.
    /// </summary>
    public abstract class AdventureContentViewModelBase : ViewModelBase
    {
        private bool _isDisposed;

        public SceneContentData Data { get; private set; }

        public SceneContentType ContentType => Data.Type;

        /// <summary>
        /// True when the item has finished preparing display data.
        /// </summary>
        public bool IsContentReady { get; private set; }

        /// <summary>
        /// Raised once when <see cref="IsContentReady"/> becomes true.
        /// </summary>
        public event Action ContentReady;

        public bool IsDisposed => _isDisposed;

        public virtual void Init(SceneContentData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            IsContentReady = true;
        }

        protected void SetContentReady()
        {
            if (IsContentReady)
                return;

            IsContentReady = true;
            ContentReady?.Invoke();
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            DisposeImplementation();
        }

        /// <summary>
        /// Override for subclass cleanup. Called once from <see cref="Dispose"/>.
        /// </summary>
        protected virtual void DisposeImplementation()
        {
        }
    }
}
