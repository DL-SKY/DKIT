using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Write API for a create-character draft. Owns wipe + reapply of derived
    /// <see cref="CharacterRequestData"/> from current Ancestry / Class / Background,
    /// then raises <see cref="CreateCharacterRequestData.NotifyUpdated"/>.
    /// Created by <c>CreateCharacterViewModel</c> and passed into sub-window VMs.
    /// Does not project UI properties and does not read formula totals
    /// (use <see cref="CreatedCharacterParametersProxy"/> for that).
    /// </summary>
    public sealed class CreateCharacterRequestApplicator
    {
        private static readonly string[] ABILITY_KEYS =
        {
            Glossary.Characters.STR,
            Glossary.Characters.DEX,
            Glossary.Characters.CON,
            Glossary.Characters.INT,
            Glossary.Characters.WIS,
            Glossary.Characters.CHA,
        };

        private static readonly string[] SKILL_KEYS =
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

        private readonly CreateCharacterRequestData _request;
        private readonly DefinitionsManager _definitionsManager;
        private readonly Dictionary<string, int> _spentAbilityBoosts = new Dictionary<string, int>();

        private bool _isDisposed;

        public IReadOnlyList<string> AbilityKeys => ABILITY_KEYS;

        public IReadOnlyList<string> SkillKeys => SKILL_KEYS;

        public bool IsDisposed => _isDisposed;

        public CreateCharacterRequestApplicator(
            CreateCharacterRequestData request,
            DefinitionsManager definitionsManager)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _definitionsManager = definitionsManager
                ?? throw new ArgumentNullException(nameof(definitionsManager));

            _request.CharacterData ??= new CharacterRequestData();
            ResetSpentAbilityBoosts();
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        /// <summary>
        /// Wipes derived character data and reapplies Features / starting equipment
        /// from the current trigger ids. Player boost spends and option-feat picks are reset.
        /// </summary>
        public void RebuildDerived(bool notify = true)
        {
            if (_isDisposed)
                return;

            _request.CharacterData ??= new CharacterRequestData();
            _request.CharacterData.Parameters = CreateBaseParameters();
            _request.CharacterData.EquippedItems = new List<EquippedItemStateData>();
            _request.CharacterData.StatusEffects = new Dictionary<string, int>();
            _request.CharacterData.Spells = new Dictionary<string, int>();
            ResetSpentAbilityBoosts();

            ApplyAncestry();
            ApplyClass();
            ApplyBackground();

            NotifyIf(notify);
        }

        public void SetName(string name, bool notify = true)
        {
            if (_isDisposed)
                return;

            string value = Normalize(name);
            if (string.Equals(Normalize(_request.Name), value, StringComparison.Ordinal))
                return;

            _request.Name = value;
            NotifyIf(notify);
        }

        public void SetAvatar(string avatar, bool notify = true)
        {
            if (_isDisposed)
                return;

            string value = Normalize(avatar);
            if (string.Equals(Normalize(_request.Avatar), value, StringComparison.Ordinal))
                return;

            _request.Avatar = value;
            NotifyIf(notify);
        }

        public void SetGender(CharacterGender gender, bool notify = true)
        {
            if (_isDisposed)
                return;

            if (_request.Gender == gender)
                return;

            _request.Gender = gender;
            NotifyIf(notify);
        }

        public void SetAncestry(string ancestryId, bool notify = true)
        {
            SetTriggerAndRebuild(ref _request.Ancestry, ancestryId, notify);
        }

        public void SetClass(string classId, bool notify = true)
        {
            SetTriggerAndRebuild(ref _request.Class, classId, notify);
        }

        public void SetBackground(string backgroundId, bool notify = true)
        {
            SetTriggerAndRebuild(ref _request.Background, backgroundId, notify);
        }

        public bool TryApplyAbilityBoost(string abilityKey, bool notify = true)
        {
            if (_isDisposed || !IsAbilityKey(abilityKey))
                return false;

            int remaining = GetBoostPoints();
            if (remaining <= 0)
                return false;

            SetRawParameter(abilityKey, GetRawParameter(abilityKey) + 1);
            SetRawParameter(Glossary.Characters.BOOST_POINTS, remaining - 1);
            _spentAbilityBoosts[abilityKey] = GetSpentAbilityBoosts(abilityKey) + 1;
            NotifyIf(notify);
            return true;
        }

        public bool TryRefundAbilityBoost(string abilityKey, bool notify = true)
        {
            if (_isDisposed || !IsAbilityKey(abilityKey))
                return false;

            int spent = GetSpentAbilityBoosts(abilityKey);
            if (spent <= 0)
                return false;

            int current = GetRawParameter(abilityKey);
            SetRawParameter(abilityKey, current - 1);
            SetRawParameter(Glossary.Characters.BOOST_POINTS, GetBoostPoints() + 1);
            _spentAbilityBoosts[abilityKey] = spent - 1;
            NotifyIf(notify);
            return true;
        }

        public int GetSpentAbilityBoosts(string abilityKey)
        {
            if (string.IsNullOrEmpty(abilityKey)
                || !_spentAbilityBoosts.TryGetValue(abilityKey, out int spent))
            {
                return 0;
            }

            return spent;
        }

        public void SetSkillProficiencyRank(string skillKey, int rank, bool notify = true)
        {
            if (_isDisposed || string.IsNullOrEmpty(skillKey))
                return;

            int clamped = ClampProficiencyRank(rank);
            string parameterKey = skillKey + Glossary.Characters.PROFICIENCY_SUFFIX;
            if (GetRawParameter(parameterKey) == clamped)
                return;

            SetRawParameter(parameterKey, clamped);
            NotifyIf(notify);
        }

        /// <summary>
        /// Applies a player-chosen option feat on the current derived snapshot.
        /// Wiped on the next <see cref="RebuildDerived"/> (ancestry / class / background change).
        /// </summary>
        public void ApplyOptionFeat(string featId, bool notify = true)
        {
            if (_isDisposed || string.IsNullOrWhiteSpace(featId))
                return;

            EnsureCharacterData();
            _request.CharacterData.ApplyFeat(featId, _definitionsManager);
            NotifyIf(notify);
        }

        public int GetRawParameter(string key)
        {
            if (_request?.CharacterData?.Parameters == null || string.IsNullOrEmpty(key))
                return 0;

            return _request.CharacterData.Parameters.TryGetValue(key, out int value) ? value : 0;
        }

        public int GetBoostPoints()
        {
            return GetRawParameter(Glossary.Characters.BOOST_POINTS);
        }

        public int GetSkillProficiencyRank(string skillKey)
        {
            if (string.IsNullOrEmpty(skillKey))
                return 0;

            return GetRawParameter(skillKey + Glossary.Characters.PROFICIENCY_SUFFIX);
        }

        private void SetTriggerAndRebuild(ref string field, string id, bool notify)
        {
            if (_isDisposed)
                return;

            string value = Normalize(id);
            if (string.Equals(Normalize(field), value, StringComparison.Ordinal))
                return;

            field = value;
            RebuildDerived(notify);
        }

        private void ApplyAncestry()
        {
            if (string.IsNullOrWhiteSpace(_request.Ancestry)
                || _definitionsManager.Ancestries == null
                || !_definitionsManager.Ancestries.TryGetValue(_request.Ancestry, out AncestryDef ancestry)
                || ancestry == null)
            {
                return;
            }

            if (ancestry.Features != null
                && ancestry.Features.TryGetValue(1, out List<string> featIds)
                && featIds != null)
            {
                ApplyFeatList(featIds);
            }

            AppendEquippedItems(ancestry.EquippedItems);
        }

        private void ApplyClass()
        {
            if (string.IsNullOrWhiteSpace(_request.Class)
                || _definitionsManager.Classes == null
                || !_definitionsManager.Classes.TryGetValue(_request.Class, out ClassDef classDef)
                || classDef == null)
            {
                return;
            }

            if (classDef.Features != null
                && classDef.Features.TryGetValue(1, out List<string> featIds)
                && featIds != null)
            {
                ApplyFeatList(featIds);
            }

            AppendEquippedItems(classDef.EquippedItems);
        }

        private void ApplyBackground()
        {
            if (string.IsNullOrWhiteSpace(_request.Background)
                || _definitionsManager.Backgrounds == null
                || !_definitionsManager.Backgrounds.TryGetValue(_request.Background, out BackgroundDef background)
                || background?.Features == null)
            {
                return;
            }

            ApplyFeatList(background.Features);
        }

        private void ApplyFeatList(List<string> featIds)
        {
            for (int i = 0; i < featIds.Count; i++)
                TryApplyAutomaticFeat(featIds[i]);
        }

        private void TryApplyAutomaticFeat(string featId)
        {
            if (string.IsNullOrWhiteSpace(featId)
                || _definitionsManager.Feats == null
                || !_definitionsManager.Feats.TryGetValue(featId, out FeatDef feat)
                || feat == null
                || feat.Disabled)
            {
                return;
            }

            if (!ShouldApplyAutomatically(feat))
                return;

            _request.CharacterData.ApplyFeat(featId, _definitionsManager);
        }

        private static bool ShouldApplyAutomatically(FeatDef feat)
        {
            if (feat.AdditionalSlots != null && feat.AdditionalSlots.Count > 0)
                return true;

            if (feat.Apply == null)
                return false;

            return (feat.Apply.Add != null && feat.Apply.Add.Count > 0)
                || (feat.Apply.Set != null && feat.Apply.Set.Count > 0)
                || (feat.Apply.AlsoApplyFeatIds != null && feat.Apply.AlsoApplyFeatIds.Count > 0)
                || feat.Apply.ConditionDuration > 0;
        }

        private void AppendEquippedItems(List<EquippedItemStateData> source)
        {
            if (source == null || source.Count == 0)
                return;

            EnsureCharacterData();
            _request.CharacterData.EquippedItems ??= new List<EquippedItemStateData>();

            for (int i = 0; i < source.Count; i++)
            {
                EquippedItemStateData item = source[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Slot))
                    continue;

                _request.CharacterData.EquippedItems.Add(
                    new EquippedItemStateData
                    {
                        Slot = item.Slot,
                        ItemId = item.ItemId,
                    });
            }
        }

        private static Dictionary<string, int> CreateBaseParameters()
        {
            var parameters = new Dictionary<string, int>
            {
                [Glossary.Characters.LEVEL] = 1,
                [Glossary.Characters.EXPERIENCE] = 0,
                [Glossary.Characters.BOOST_POINTS] = 0,
            };

            for (int i = 0; i < ABILITY_KEYS.Length; i++)
                parameters[ABILITY_KEYS[i]] = 0;

            return parameters;
        }

        private void SetRawParameter(string key, int value)
        {
            EnsureCharacterData();
            _request.CharacterData.Parameters ??= new Dictionary<string, int>();
            _request.CharacterData.Parameters[key] = value;
        }

        private void EnsureCharacterData()
        {
            _request.CharacterData ??= new CharacterRequestData();
            _request.CharacterData.Parameters ??= new Dictionary<string, int>();
        }

        private void ResetSpentAbilityBoosts()
        {
            _spentAbilityBoosts.Clear();
            for (int i = 0; i < ABILITY_KEYS.Length; i++)
                _spentAbilityBoosts[ABILITY_KEYS[i]] = 0;
        }

        private static bool IsAbilityKey(string abilityKey)
        {
            if (string.IsNullOrEmpty(abilityKey))
                return false;

            for (int i = 0; i < ABILITY_KEYS.Length; i++)
            {
                if (string.Equals(ABILITY_KEYS[i], abilityKey, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static int ClampProficiencyRank(int rank)
        {
            int min = (int)ProficiencyType.Untrained;
            int max = (int)ProficiencyType.Legendary;
            if (rank < min)
                return min;
            if (rank > max)
                return max;
            return rank;
        }

        private static string Normalize(string value)
        {
            return value ?? string.Empty;
        }

        private void NotifyIf(bool notify)
        {
            if (notify)
                _request.NotifyUpdated();
        }
    }
}
