using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Definitions.Scripts.Implementation.Defs.GameZoneGems;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Definitions.Scripts.Implementation.Defs.Rounds;
using Modules.ECS.Scripts.Match3.Systems.Actions;
using Modules.ECS.Scripts.Match3.Systems.Events;
using Modules.ECS.Scripts.Match3.Systems.Init;
using Modules.ECS.Scripts.Match3.Systems.Match;
using Modules.ECS.Scripts.Match3.Systems.Move;
using Modules.ECS.Scripts.Match3.Systems.Objectives;
using Modules.ECS.Scripts.Match3.Systems.Settings;
using Modules.Match3.Scripts.Core;
using Modules.Match3.Scripts.Implementation.Data;
using Modules.Utils.Scripts.Components;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Match3.Scripts.Implementation.Core
{
    public class Match3RoundController : RoundControllerBase
    {
        //[Inject] private readonly WindowsManager _windowsManager;

        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly Updater _updater;
        [Inject] private readonly EcsSystemFactory _ecsSystemFactory;

        public EcsWorld World => _world;
        public EcsSystems Systems => _systems;

        private EcsWorld _world;
        private EcsSystems _systems;

        private RoundDef _roundDef;
        private GameZoneDef _gameZoneDef;
        private GameZoneGemsDef _gameZoneGemsDef;
        

        public void Init(string defId)
        {
            UnityEngine.Debug.LogError($"Match3RoundController.Init({defId})");

            if (!_definitionsManager.Rounds.TryGetValue(defId, out var roundDef))
            {
                UnityEngine.Debug.LogError($"[Match3RoundController] Init({defId}) :: Not found Rounds def with ID \"{defId}\"!");
                return;
            }
            _roundDef = roundDef;

            if (!_definitionsManager.GameZones.TryGetValue(_roundDef.GameZone, out var gameZoneDef))
            {
                UnityEngine.Debug.LogError($"[Match3RoundController] Init({defId}) :: Not found GameZones def with ID \"{_roundDef.GameZone}\"!");
                return;
            }
            _gameZoneDef = gameZoneDef;

            if (!_definitionsManager.GameZoneGems.TryGetValue(_roundDef.Gems, out var gameZoneGemsDef))
            {
                UnityEngine.Debug.LogError($"[Match3RoundController] Init({defId}) :: Not found GameZoneGems def with ID \"{_roundDef.Gems}\"!");
                return;
            }
            _gameZoneGemsDef = gameZoneGemsDef;

            var gameZoneData = new GameZoneData(_gameZoneDef);
            var gemsData = new GemsData(_gameZoneGemsDef);
            var objectivesData = new ObjectivesData();
            InitBase(gameZoneData, gemsData, objectivesData);
        }


        protected override void InitImplementation()
        {
            // Инициализируем ECS
            _world = new EcsWorld();
            _systems = new EcsSystems(_world);

            // Создаем систему хранения глобальных настроек для Match-3
            var match3GlobalSettingsSystem = _ecsSystemFactory.Create<Match3GlobalSettingsSystem>();
            // Создаем систему вычисления отступа игрового поля относительно центра экрана и камеры
            var centeringOffsetCalculateSystem = _ecsSystemFactory.Create<CenteringOffsetCalculateSystem>(new object[] { _gameZoneData });
            // Создаем систему настройки камеры
            var cameraSetupSystem = _ecsSystemFactory.Create<CameraSetupSystem>(new object[] { _gameZoneData });

            // Создаем систему инициализации счетчика ходов
            var turnsInitSystem = _ecsSystemFactory.Create<TurnsInitSystem>(new object[] { _objectivesData });
            // Создаем систему инициализации счетчика очков, целей и т.д.
            var scoreInitSystem = _ecsSystemFactory.Create<ScoreInitSystem>(new object[] { _objectivesData });
            // Создаем систему инициализации клеток
            var cellsInitSystem = _ecsSystemFactory.Create<CellsInitSystem>(new object[] { _gameZoneData });
            // Создаем систему инициализации фишек
            var gemsInitSystem = _ecsSystemFactory.Create<GemsInitSystem>(new object[] { _gameZoneData, _gemsData });

            // Создаем систему работы с коллбэками
            var callbackSystem = _ecsSystemFactory.Create<CallbackSystem>();
            // Создаем систему учета ходов
            var turnsSystem = _ecsSystemFactory.Create<TurnsSystem>();
            // Создаем систему учета очков / задач / прогресса
            var scoreSystem = _ecsSystemFactory.Create<ScoreSystem>();
            // Создаем систему обработки match-запросов
            var matchScoreRequestProcessSystem = _ecsSystemFactory.Create<MatchScoreRequestProcessSystem>();
            // Создаем систему обработки экшен-запросов
            var actionProcessSystem = _ecsSystemFactory.Create<ActionProcessSystem>();

            // Создаем системы для перетаскивания фишек
            var dragStartSystem = _ecsSystemFactory.Create<DragStartSystem>();
            var dragEndSystem = _ecsSystemFactory.Create<DragEndSystem>();
            var swapSystem = _ecsSystemFactory.Create<SwapSystem>();
            var swapAnimationSystem = _ecsSystemFactory.Create<SwapAnimationSystem>();
            // Создаем систему определения совпадений
            var matchDetectionSystem = _ecsSystemFactory.Create<MatchDetectionSystem>(new object[] { _gameZoneData });
            // Создаем систему удаления фишек после совпадений
            var matchDestructionSystem = _ecsSystemFactory.Create<MatchDestructionSystem>();
            // Создаем систему создания новых фишек после удаления
            var gemsSpawnSystem = _ecsSystemFactory.Create<GemsSpawnSystem>(new object[] { _gameZoneData, _gemsData });
            // Создаем системы для падения фишек
            var fallSystem = _ecsSystemFactory.Create<FallSystem>(new object[] { _gameZoneData });
            var fallAnimationSystem = _ecsSystemFactory.Create<FallAnimationSystem>();
            //...

            _systems
                .Add(match3GlobalSettingsSystem)
                .Add(centeringOffsetCalculateSystem)
                .Add(cameraSetupSystem)
                
                .Add(turnsInitSystem)
                .Add(scoreInitSystem)
                .Add(cellsInitSystem)
                .Add(gemsInitSystem)

                .Add(callbackSystem)
                .Add(turnsSystem)
                .Add(scoreSystem)
                .Add(matchScoreRequestProcessSystem)
                .Add(actionProcessSystem)
                
                .Add(dragStartSystem)
                .Add(dragEndSystem)
                .Add(swapSystem)
                .Add(swapAnimationSystem)
                .Add(matchDetectionSystem)
                .Add(matchDestructionSystem)
                .Add(gemsSpawnSystem)
                .Add(fallSystem)
                .Add(fallAnimationSystem)

                .Init();



            // TMP: использование фильтров в других объектах, н-р, в VM
            //
            //EcsFilter filter;
            //
            //filter = _world.GetFilter(typeof(EcsFilter<MatchScoreRequest>));
            //UnityEngine.Debug.LogError($"OnUpdateHandler() : filter: {filter.GetEntitiesCount()}");
            //foreach (var i in filter)
            //{
            //    ref var entity = ref filter.GetEntity(i);
            //    UnityEngine.Debug.LogError($"     filter<MatchScoreRequest>: {entity.Has<MatchScoreRequest>()} / {entity.Get<MatchScoreRequest>().GemType}");
            //}

        }


        protected override void Subscribe()
        {
            _updater.OnUpdate += OnUpdateHandler;
        }

        protected override void Unsubscribe()
        {
            _updater.OnUpdate -= OnUpdateHandler;
        }

        private void OnUpdateHandler(float deltaTime)
        {
            _systems?.Run();
        }

        public override void Dispose()
        {
            // Очищаем ECS
            _systems?.Destroy();
            _systems = null;
            _world?.Destroy();
            _world = null;

            Unsubscribe();
        }
    }
}
