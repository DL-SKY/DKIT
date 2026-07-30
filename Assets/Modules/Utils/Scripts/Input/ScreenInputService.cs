using System;
using Modules.Utils.Scripts.Components;
using UnityEngine;
using Zenject;

namespace Modules.Utils.Scripts.Input
{
    /// <summary>
    /// Project-wide screen input hub. Polls pointer state via <see cref="Updater"/>
    /// and raises typed events for any consumer (adventure scroll skip, future listeners, etc.).
    /// </summary>
    public sealed class ScreenInputService : IDisposable
    {
        private readonly Updater _updater;
        private bool _isDisposed;

        public event Action<ScreenInputType> OnInput;

        [Inject]
        public ScreenInputService(Updater updater)
        {
            _updater = updater ?? throw new ArgumentNullException(nameof(updater));
            _updater.OnUpdate += OnUpdateHandler;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_updater != null)
                _updater.OnUpdate -= OnUpdateHandler;

            OnInput = null;
        }

        private void OnUpdateHandler(float deltaTime)
        {
            if (!WasPointerDownThisFrame())
                return;

            OnInput?.Invoke(ScreenInputType.PointerDown);
        }

        /// <summary>
        /// Prefers raw touches when present to avoid double-firing with simulated mouse on mobile.
        /// Falls back to mouse button 0 when there are no active touches.
        /// </summary>
        private static bool WasPointerDownThisFrame()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    if (UnityEngine.Input.GetTouch(i).phase == TouchPhase.Began)
                        return true;
                }

                return false;
            }

            return UnityEngine.Input.GetMouseButtonDown(0);
        }
    }
}
