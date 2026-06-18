using Modules.Definitions.Scripts.Core;
using Modules.Definitions.Scripts.Implementation.Defs.Cells;
using Modules.Definitions.Scripts.Implementation.Defs.GameZoneGems;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;
using Modules.Definitions.Scripts.Implementation.Defs.Objectives;
using Modules.Definitions.Scripts.Implementation.Defs.Presets;
using Modules.Definitions.Scripts.Implementation.Defs.Rounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Adventures;
using Modules.Definitions.Scripts.Implementation.Defs.Single;
using Modules.Utils.Scripts.Components;
using Modules.Utils.Scripts.UnityImplementation;
using System;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Modules.Definitions.Scripts.Implementation.Adventures
{
    public class DefinitionsManager
    {
        private const int LOAD_METHODS_BATCH = 10;

        [Inject] private readonly CoroutineHolder _coroutineHolder;

        private readonly Loader _loader;
        private readonly SimpleAsyncOperation _asyncOperation;


        public ProjectGlobalSettingsDef GlobalSettings;
        public Dictionary<string, AdventureDef> Adventures;
        //public Match3GlobalSettingsDef Match3GlobalSettings;
        //public Dictionary<string, GameZoneDef> GameZones;
        //public CellsMapDef CellsMap;
        //public Dictionary<string, CellDef> Cells;
        //public CellsMapDef PresetsMap;
        //public Dictionary<string, PresetDef> Presets;
        //public Dictionary<string, GemDef> Gems;
        //public Dictionary<string, GameZoneGemsDef> GameZoneGems;
        //public Dictionary<string, ObjectivesDef> Objectives;
        //public Dictionary<string, RoundDef> Rounds;


        public DefinitionsManager()
        {
            _loader = new Loader();
            _asyncOperation = new SimpleAsyncOperation();
        }


        public SimpleAsyncOperation InitAsync()
        {
            _coroutineHolder.StartCoroutine(LoadAll());
            return _asyncOperation;
        }

        private IEnumerator LoadAll()
        {
            // Заполняем список методов-инициализаторов дэфов
            var loadMethods = new List<Action>
            {
                LoadGlobalSettings,
                LoadAdventures,
                //LoadMatch3GlobalSettings,
                //LoadGameZones,
                //LoadCellsMap,
                //LoadCells,
                //LoadPresetsMap,
                //LoadPresets,
                //LoadGems,
                //LoadGameZoneGems,
                //LoadObjectives,
                //LoadRounds,
            };            

            for (int i = 0; i < loadMethods.Count; i++)
            {
                loadMethods[i].Invoke();
                UnityEngine.Debug.Log($"[DefinitionsManager] Загрузка настроек в {loadMethods[i].Method.Name} завершена.");

                if ((i + 1) % LOAD_METHODS_BATCH == 0)
                    yield return null;
            }

            _asyncOperation.SetCompleted();
        }

        private void LoadGlobalSettings()
        {
            GlobalSettings = _loader.LoadSingle<ProjectGlobalSettingsDef>("Definitions/_ADVENTURES_/GlobalSettings/GlobalSettings");
        }

        private void LoadAdventures()
        {
            Adventures = _loader.LoadCollection<AdventureDef>("Definitions/_ADVENTURES_/Adventures");
        }

        //private void LoadMatch3GlobalSettings()
        //{
        //    Match3GlobalSettings = _loader.LoadSingle<Match3GlobalSettingsDef>("Definitions/Match3GlobalSettings/Match3GlobalSettings");
        //}

        //private void LoadGameZones()
        //{
        //    GameZones = _loader.LoadCollection<GameZoneDef>("Definitions/GameZones");
        //}

        //private void LoadCellsMap()
        //{
        //    CellsMap = _loader.LoadSingle<CellsMapDef>("Definitions/CellsMap/CellsMap");
        //}

        //private void LoadCells()
        //{
        //    Cells = _loader.LoadCollection<CellDef>("Definitions/Cells");
        //}

        //private void LoadPresetsMap()
        //{
        //    PresetsMap = _loader.LoadSingle<CellsMapDef>("Definitions/PresetsMap/PresetsMap");
        //}

        //private void LoadPresets()
        //{
        //    Presets = _loader.LoadCollection<PresetDef>("Definitions/Presets");
        //}

        //private void LoadGems()
        //{
        //    Gems = _loader.LoadCollection<GemDef>("Definitions/Gems");
        //}

        //private void LoadGameZoneGems()
        //{
        //    GameZoneGems = _loader.LoadCollection<GameZoneGemsDef>("Definitions/GameZoneGems");
        //}

        //private void LoadObjectives()
        //{
        //    Objectives = _loader.LoadCollection<ObjectivesDef>("Definitions/Objectives");
        //}

        //private void LoadRounds()
        //{ 
        //    Rounds = _loader.LoadCollection<RoundDef>("Definitions/Rounds");
        //}
    }
}
