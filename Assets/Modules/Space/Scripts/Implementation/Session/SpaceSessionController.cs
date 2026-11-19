using System;
using Leopotam.Ecs;
using Modules.ECS.Scripts.Space.Systems;
using Modules.Utils.Scripts.Components;
using Zenject.Scripts.Factories;

namespace Modules.Space.Scripts.Implementation.Session
{
    /// <summary>
    /// Owns the flight ECS world for one sector visit. This pass has a clock and one session entity.
    /// </summary>
    public sealed class SpaceSessionController : IDisposable
    {
        private readonly Updater _updater;
        private readonly EcsSystemFactory _ecsSystemFactory;
        private readonly SpaceFrameTime _frameTime = new SpaceFrameTime();

        private EcsWorld _world;
        private EcsSystems _systems;
        private bool _subscribed;

        public SpaceSessionController(Updater updater, EcsSystemFactory ecsSystemFactory)
        {
            _updater = updater;
            _ecsSystemFactory = ecsSystemFactory;
        }

        public void Start()
        {
            if (_world != null)
            {
                UnityEngine.Debug.LogWarning("[Space] Session world is already running.");
                return;
            }

            if (_updater == null || _ecsSystemFactory == null)
            {
                UnityEngine.Debug.LogError("[Space] Session world was not started. Updater or system factory is missing.");
                return;
            }

            _world = new EcsWorld();
            _systems = new EcsSystems(_world);

            try
            {
                var sessionInitSystem = _ecsSystemFactory.Create<SpaceSessionInitSystem>();
                var clockSystem = _ecsSystemFactory.Create<SpaceClockSystem>(new object[] { _frameTime });

                _systems
                    .Add(sessionInitSystem)
                    .Add(clockSystem)
                    .Init();

                _updater.OnUpdate += OnUpdate;
                _subscribed = true;
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"[Space] Session world failed to start. {exception.Message}");
                Dispose();
                return;
            }

            UnityEngine.Debug.Log("[Space] Session world started. No contacts spawned.");
        }

        public void Dispose()
        {
            if (_subscribed && _updater != null)
            {
                _updater.OnUpdate -= OnUpdate;
                _subscribed = false;
            }

            if (_systems != null)
            {
                _systems.Destroy();
                _systems = null;
            }

            if (_world != null)
            {
                _world.Destroy();
                _world = null;
                UnityEngine.Debug.Log("[Space] Session world disposed.");
            }
        }

        private void OnUpdate(float deltaTime)
        {
            _frameTime.DeltaSeconds = deltaTime;
            _systems?.Run();
        }
    }
}
