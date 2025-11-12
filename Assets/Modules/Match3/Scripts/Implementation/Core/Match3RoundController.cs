using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.ECS.Scripts.Match3.Systems;
using Modules.Match3.Scripts.Core;
using Modules.Windows.Scripts.Managers;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Match3.Scripts.Implementation.Core
{
    public class Match3RoundController : RoundControllerBase
    {
        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly EcsSystemFactory _ecsSystemFactory;

        private GameZoneDef _def;
        private EcsWorld _world;
        private EcsSystems _systems;


        public void Init(string defId)
        {
            UnityEngine.Debug.LogError($"Match3RoundController.Init({defId})");

            if (!_definitionsManager.GameZones.TryGetValue(defId, out var def))
            {
                UnityEngine.Debug.LogError($"[Match3RoundController.Init({defId})] Not found GameZones def with ID \"{defId}\"!");
                return;
            }

            _def = def;

            InitBase(_def);
        }


        protected override void InitImplementation()
        {
            // Инициализируем ECS
            _world = new EcsWorld();
            _systems = new EcsSystems(_world);

            // Создаем систему инициализации клеток
            var cellInitSystem = _ecsSystemFactory.Create<CellInitSystem>(new object[] { _data });

            _systems
                .Add(cellInitSystem)
                .Init();
        }

        protected override void Subscribe()
        {
            
        }

        protected override void Unsubscribe()
        {
            
        }

        


        public override void Dispose()
        {
            UnityEngine.Debug.LogError($"Match3RoundController.Dispose()");

            // Очищаем ECS
            _systems?.Destroy();
            _systems = null;
            _world?.Destroy();
            _world = null;
        }
    }
}
