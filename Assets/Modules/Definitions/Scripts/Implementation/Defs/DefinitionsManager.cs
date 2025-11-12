using Modules.Definitions.Scripts.Core;
using Modules.Definitions.Scripts.Implementation.Defs.Cells;
using Modules.Definitions.Scripts.Implementation.Defs.GameZones;
using Modules.Definitions.Scripts.Implementation.Defs.Presets;
using Modules.Definitions.Scripts.Implementation.Defs.Single;
using Modules.Utils.Scripts.Components;
using Modules.Utils.Scripts.UnityImplementation;
using System;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Modules.Definitions.Scripts.Implementation.Defs
{
    public class DefinitionsManager
    {
        [Inject] private readonly CoroutineHolder _coroutineHolder;

        private readonly Loader _loader;
        private readonly SimpleAsyncOperation _asyncOperation;

        public Dictionary<string, GameZoneDef> GameZones;
        public CellsMapDef CellsMap;
        public Dictionary<string, CellDef> Cells;
        public CellsMapDef PresetsMap;
        public Dictionary<string, PresetDef> Presets;


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
            var loadMethods = new List<Action>{
                LoadGameZones,
                LoadCellsMap,
                LoadCells,
                LoadPresetsMap,
                LoadPresets,
            };            

            foreach (var method in loadMethods)
            {
                method.Invoke();
                yield return null;
            }

            _asyncOperation.SetCompleted();
        }

        private void LoadGameZones()
        {
            GameZones = _loader.LoadCollection<GameZoneDef>("Definitions/GameZones");
        }

        private void LoadCellsMap()
        {
            CellsMap = _loader.LoadSingle<CellsMapDef>("Definitions/CellsMap/CellsMap");
        }

        private void LoadCells()
        {
            Cells = _loader.LoadCollection<CellDef>("Definitions/Cells");
        }

        private void LoadPresetsMap()
        {
            PresetsMap = _loader.LoadSingle<CellsMapDef>("Definitions/PresetsMap/PresetsMap");
        }

        private void LoadPresets()
        {
            Presets = _loader.LoadCollection<PresetDef>("Definitions/Presets");
        }
    }
}
