using Modules.Utils.Scripts.Components;
using UnityEngine;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Keeps the sector world alive after the startup scene unloads.
    /// </summary>
    public sealed class SpaceSessionHost : MonoBehaviour
    {
        private SpaceSessionController _session;

        public void StartSession(Updater updater, EcsSystemFactory factory)
        {
            if (_session != null)
                return;

            _session = new SpaceSessionController(updater, factory);
            _session.Start();
        }

        private void OnDestroy()
        {
            _session?.Dispose();
            _session = null;
        }
    }
}
