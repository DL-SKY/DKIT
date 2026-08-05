using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using System.Collections.Generic;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter
{
    /// <summary>
    /// Cursor-only helper: rebuilds draft <see cref="CharacterRequestData.Parameters"/>
    /// from selected Ancestry / Class / Background level-1 features.
    /// </summary>
    public static class CursorCreateCharacterDraftApplier
    {
        private static readonly string[] AbilityKeys =
        {
            Glossary.Characters.STR,
            Glossary.Characters.DEX,
            Glossary.Characters.CON,
            Glossary.Characters.INT,
            Glossary.Characters.WIS,
            Glossary.Characters.CHA,
        };

        public static readonly string[] SkillKeys =
        {
            Glossary.Characters.ACROBATICS,
            Glossary.Characters.ARCANA,
            Glossary.Characters.ATHLETICS,
            Glossary.Characters.CRAFTING,
            Glossary.Characters.DECEPTION,
            Glossary.Characters.DIPLOMACY,
            Glossary.Characters.INTIMIDATION,
            Glossary.Characters.LORE,
            Glossary.Characters.MEDICINE,
            Glossary.Characters.NATURE,
            Glossary.Characters.OCCULTISM,
            Glossary.Characters.PERFORMANCE,
            Glossary.Characters.RELIGION,
            Glossary.Characters.SOCIETY,
            Glossary.Characters.STEALTH,
            Glossary.Characters.SURVIVAL,
            Glossary.Characters.THIEVERY,
        };

        public static IReadOnlyList<string> AbilityKeysReadonly => AbilityKeys;

        /// <summary>
        /// Rebuilds parameters from definitions. Player-allocated free boosts / skill picks are reset.
        /// </summary>
        public static void RebuildFromSelections(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager)
        {
            if (request == null)
                return;

            request.CharacterData ??= new CharacterRequestData();
            request.CharacterData.Parameters = CreateBaseParameters();
            request.CharacterData.EquippedItems = new List<Modules.State.Scripts.Implementation.Adventure.StateDatas.EquippedItemStateData>();
            request.CharacterData.StatusEffects = new Dictionary<string, int>();
            request.CharacterData.Spells = new Dictionary<string, int>();

            if (definitionsManager == null)
                return;

            ApplyAncestry(request, definitionsManager);
            ApplyClass(request, definitionsManager);
            ApplyBackground(request, definitionsManager);
        }

        public static int GetRawParameter(CreateCharacterRequestData request, string key)
        {
            if (request?.CharacterData?.Parameters == null || string.IsNullOrEmpty(key))
                return 0;

            return request.CharacterData.Parameters.TryGetValue(key, out int value) ? value : 0;
        }

        public static void SetRawParameter(CreateCharacterRequestData request, string key, int value)
        {
            if (request == null || string.IsNullOrEmpty(key))
                return;

            request.CharacterData ??= new CharacterRequestData();
            request.CharacterData.Parameters ??= new Dictionary<string, int>();
            request.CharacterData.Parameters[key] = value;
        }

        public static int GetBoostPoints(CreateCharacterRequestData request)
        {
            return GetRawParameter(request, Glossary.Characters.BOOST_POINTS);
        }

        public static int GetSkillProficiencyRank(CreateCharacterRequestData request, string skillKey)
        {
            return GetRawParameter(request, skillKey + Glossary.Characters.PROFICIENCY_SUFFIX);
        }

        public static void SetSkillProficiencyRank(
            CreateCharacterRequestData request,
            string skillKey,
            int rank)
        {
            SetRawParameter(request, skillKey + Glossary.Characters.PROFICIENCY_SUFFIX, rank);
        }

        private static Dictionary<string, int> CreateBaseParameters()
        {
            var parameters = new Dictionary<string, int>
            {
                [Glossary.Characters.LEVEL] = 1,
                [Glossary.Characters.EXPERIENCE] = 0,
                [Glossary.Characters.BOOST_POINTS] = 0,
            };

            for (int i = 0; i < AbilityKeys.Length; i++)
                parameters[AbilityKeys[i]] = 0;

            return parameters;
        }

        private static void ApplyAncestry(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager)
        {
            if (string.IsNullOrWhiteSpace(request.Ancestry)
                || definitionsManager.Ancestries == null
                || !definitionsManager.Ancestries.TryGetValue(request.Ancestry, out AncestryDef ancestry)
                || ancestry == null
                || ancestry.Features == null)
            {
                return;
            }

            if (!ancestry.Features.TryGetValue(1, out List<string> featIds) || featIds == null)
                return;

            ApplyFeatList(request, definitionsManager, featIds);
            AppendEquippedItems(request, ancestry.EquippedItems);
        }

        private static void ApplyClass(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager)
        {
            if (string.IsNullOrWhiteSpace(request.Class)
                || definitionsManager.Classes == null
                || !definitionsManager.Classes.TryGetValue(request.Class, out ClassDef classDef)
                || classDef == null)
            {
                return;
            }

            if (classDef.Features != null
                && classDef.Features.TryGetValue(1, out List<string> featIds)
                && featIds != null)
            {
                ApplyFeatList(request, definitionsManager, featIds);
            }

            AppendEquippedItems(request, classDef.EquippedItems);
        }

        private static void ApplyBackground(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager)
        {
            if (string.IsNullOrWhiteSpace(request.Background)
                || definitionsManager.Backgrounds == null
                || !definitionsManager.Backgrounds.TryGetValue(request.Background, out BackgroundDef background)
                || background?.Features == null)
            {
                return;
            }

            ApplyFeatList(request, definitionsManager, background.Features);
        }

        private static void ApplyFeatList(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager,
            List<string> featIds)
        {
            for (int i = 0; i < featIds.Count; i++)
                TryApplyFeat(request, definitionsManager, featIds[i]);
        }

        private static void TryApplyFeat(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager,
            string featId)
        {
            if (string.IsNullOrWhiteSpace(featId)
                || definitionsManager.Feats == null
                || !definitionsManager.Feats.TryGetValue(featId, out FeatDef feat)
                || feat == null
                || feat.Disabled)
            {
                return;
            }

            bool hasOptions = feat.Options != null && feat.Options.Count > 0;
            bool hasApplyPatch = feat.Apply != null
                && ((feat.Apply.Add != null && feat.Apply.Add.Count > 0)
                    || (feat.Apply.Set != null && feat.Apply.Set.Count > 0)
                    || (feat.Apply.AlsoApplyFeatIds != null && feat.Apply.AlsoApplyFeatIds.Count > 0));

            // Free boost choice feats currently have Options but no Apply that writes BoostPoints.
            // Cursor treats them as +1 free BoostPoints until FeatDefs write Glossary.Characters.BOOST_POINTS.
            if (hasOptions && feat.Type == FeatType.Boost && !hasApplyPatch)
            {
                int boostPoints = GetBoostPoints(request);
                SetRawParameter(request, Glossary.Characters.BOOST_POINTS, boostPoints + 1);
                return;
            }

            if (hasApplyPatch)
                request.CharacterData.ApplyFeat(featId, definitionsManager);

            // Choice feats without an auto Apply patch are left for dedicated pickers (skills / heritage / etc.).
        }

        private static void AppendEquippedItems(
            CreateCharacterRequestData request,
            List<Modules.State.Scripts.Implementation.Adventure.StateDatas.EquippedItemStateData> source)
        {
            if (source == null || source.Count == 0)
                return;

            request.CharacterData.EquippedItems ??=
                new List<Modules.State.Scripts.Implementation.Adventure.StateDatas.EquippedItemStateData>();

            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Slot))
                    continue;

                request.CharacterData.EquippedItems.Add(
                    new Modules.State.Scripts.Implementation.Adventure.StateDatas.EquippedItemStateData
                    {
                        Slot = item.Slot,
                        ItemId = item.ItemId,
                    });
            }
        }
    }
}
