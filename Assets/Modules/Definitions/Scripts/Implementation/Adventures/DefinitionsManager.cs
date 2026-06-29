using Modules.Definitions.Scripts.Core;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleRules;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Rules;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Single;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Spells;
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
        public LocalizationSettingsDef LocalizationSettings;
        public RuleSettingsDef RuleSettings;
        public Dictionary<string, AdventureDef> Adventures;
        public Dictionary<string, ClassDef> Classes;
        public Dictionary<string, AncestryDef> Ancestries;
        public Dictionary<string, FeatDef> Feats;
        public Dictionary<string, ItemDef> Items;
        public Dictionary<string, SpellDef> Spells;
        public Dictionary<string, RuleDef> Rules;
        public Dictionary<string, BattleRuleDef> BattleRules;


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
                LoadLocalizationSettings,
                
                LoadRules,
                LoadBattleRules,
                LoadRuleSettings,

                LoadAdventures,

                LoadClasses,
                LoadAncestries,
                LoadFeats,
                LoadItems,
                LoadSpells,                
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

        private void LoadLocalizationSettings()
        {
            LocalizationSettings = _loader.LoadSingle<LocalizationSettingsDef>("Definitions/_ADVENTURES_/LocalizationSettings/LocalizationSettings");
        }

        private void LoadClasses()
        {
            Classes = _loader.LoadCollection<ClassDef>("Definitions/_ADVENTURES_/Classes");
        }

        private void LoadAncestries()
        {
            Ancestries = _loader.LoadCollection<AncestryDef>("Definitions/_ADVENTURES_/Ancestries");
        }

        private void LoadFeats()
        {
            Feats = _loader.LoadCollection<FeatDef>("Definitions/_ADVENTURES_/Feats");
        }

        private void LoadItems()
        {
            Items = _loader.LoadCollection<ItemDef>("Definitions/_ADVENTURES_/Items");
        }

        private void LoadSpells()
        {
            Spells = _loader.LoadCollection<SpellDef>("Definitions/_ADVENTURES_/Spells");
        }

        private void LoadRules()
        {
            Rules = _loader.LoadCollection<RuleDef>("Definitions/_ADVENTURES_/Rules");
        }

        private void LoadBattleRules()
        {
            BattleRules = _loader.LoadCollection<BattleRuleDef>("Definitions/_ADVENTURES_/BattleRules");
        }

        private void LoadRuleSettings()
        {
            RuleSettings = _loader.LoadSingle<RuleSettingsDef>("Definitions/_ADVENTURES_/RuleSettings/RuleSettings");
        }
    }
}
