using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Creatures;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Utils.Scripts.Extensions;
using System;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Builds battle combatants as <see cref="CharacterStateData"/> from <see cref="CreatureDef"/>
    /// (and clones party characters for session copies).
    /// </summary>
    /// <remarks>
    /// <para>
    /// NPC instance ids must be negative so they never collide with
    /// <see cref="CharactersStateData.NextCharacterId"/> party ids.
    /// Does not write into profile <see cref="CharactersStateData"/>.
    /// </para>
    /// <para>
    /// Practical guide: <c>.cursor/docs/modules/Creatures.md</c>.
    /// </para>
    /// <example>
    /// Spawn an NPC and a party copy for a battle session:
    /// <code>
    /// // Opponent from stat block (session ids: -1, -2, ...)
    /// CreatureDef goblinDef = definitionsManager.Creatures["_GoblinWarrior"];
    /// CharacterStateData goblin = CreatureCombatantFactory.CreateFromCreature(
    ///     goblinDef,
    ///     instanceId: -1,
    ///     definitionsManager);
    ///
    /// // Party member copy (do not mutate the saved character mid-combat)
    /// CharacterStateData heroCopy = CreatureCombatantFactory.CloneForBattle(
    ///     state.Characters.Characters[heroId]);
    ///
    /// // Same status Write API for both sides
    /// CharacterParametersOperator.ApplyFeat(goblin, "_ConditionFrightened", definitionsManager);
    /// CharacterParametersOperator.ApplyFeat(heroCopy, "_ConditionFrightened", definitionsManager);
    ///
    /// // Battle actions stay on the def
    /// List&lt;string&gt; actions = goblinDef.BattleActionIds;
    /// </code>
    /// </example>
    /// </remarks>
    public static class CreatureCombatantFactory
    {
        /// <summary>
        /// Materializes a creature stat block into a valid battle <see cref="CharacterStateData"/>.
        /// Does not write into profile <see cref="CharactersStateData"/>.
        /// </summary>
        /// <param name="creatureDef">Source stat block.</param>
        /// <param name="instanceId">Session id; must be &lt; 0 for NPC combatants.</param>
        /// <param name="definitionsManager">Needed for <c>ApplyFeat</c> / item Features.</param>
        /// <returns>
        /// Ready combatant, or <c>null</c> if <paramref name="creatureDef"/> is null.
        /// </returns>
        /// <example>
        /// <code>
        /// CharacterStateData wolf = CreatureCombatantFactory.CreateFromCreature(
        ///     definitionsManager.Creatures["_Wolf"],
        ///     instanceId: -2,
        ///     definitionsManager);
        /// </code>
        /// </example>
        public static CharacterStateData CreateFromCreature(
            CreatureDef creatureDef,
            int instanceId,
            DefinitionsManager definitionsManager)
        {
            if (creatureDef == null)
            {
                UnityEngine.Debug.LogError("[CreatureCombatantFactory] CreatureDef is null.");
                return null;
            }

            if (instanceId >= 0)
            {
                UnityEngine.Debug.LogWarning(
                    $"[CreatureCombatantFactory] NPC instance id should be negative " +
                    $"(got {instanceId} for '{creatureDef.Id}').");
            }

            var character = new CharacterStateData
            {
                Id = instanceId,
                CreateTime = DateTime.UtcNow.ToUnixMs(),
                IsDead = false,
                DeathTime = 0,
                Avatar = creatureDef.Icon ?? string.Empty,
                Name = string.IsNullOrWhiteSpace(creatureDef.Title)
                    ? creatureDef.Id
                    : creatureDef.Title,
                Gender = creatureDef.Gender,
                Ancestry = creatureDef.Ancestry ?? string.Empty,
                Class = creatureDef.Class ?? string.Empty,
                Background = creatureDef.Background ?? string.Empty,
                Parameters = new Dictionary<string, int>(),
                EquippedItems = CloneEquippedItems(creatureDef.EquippedItems),
                Spells = CloneDictionary(creatureDef.Spells),
                StatusEffects = new Dictionary<string, int>(),
            };

            BakeStatBlock(character.Parameters, creatureDef);

            if (creatureDef.Features != null)
            {
                for (int i = 0; i < creatureDef.Features.Count; i++)
                {
                    string featId = creatureDef.Features[i];
                    if (string.IsNullOrWhiteSpace(featId))
                        continue;

                    CharacterParametersOperator.ApplyFeat(character, featId, definitionsManager);
                }
            }

            ApplyWornItemFeatures(character, definitionsManager);

            return character;
        }

        /// <summary>
        /// Deep-ish copy of a character for battle session (party side).
        /// Keeps the same <see cref="CharacterStateData.Id"/> unless <paramref name="overrideId"/> is set.
        /// </summary>
        /// <param name="source">Party (or any) character to copy.</param>
        /// <param name="overrideId">Optional id override; default keeps <paramref name="source"/>.Id.</param>
        /// <example>
        /// <code>
        /// CharacterStateData copy = CreatureCombatantFactory.CloneForBattle(
        ///     state.Characters.Characters[heroId]);
        /// </code>
        /// </example>
        public static CharacterStateData CloneForBattle(
            CharacterStateData source,
            int? overrideId = null)
        {
            if (source == null)
                return null;

            return new CharacterStateData
            {
                Id = overrideId ?? source.Id,
                CreateTime = source.CreateTime,
                IsDead = source.IsDead,
                DeathTime = source.DeathTime,
                Avatar = source.Avatar,
                Name = source.Name,
                Gender = source.Gender,
                Ancestry = source.Ancestry,
                Class = source.Class,
                Background = source.Background,
                Parameters = CloneDictionary(source.Parameters),
                EquippedItems = CloneEquippedItems(source.EquippedItems),
                Spells = CloneDictionary(source.Spells),
                StatusEffects = CloneDictionary(source.StatusEffects),
            };
        }

        private static void BakeStatBlock(
            Dictionary<string, int> parameters,
            CreatureDef creatureDef)
        {
            int level = creatureDef.Level;
            parameters[Glossary.Characters.LEVEL] = level;
            parameters[Glossary.Characters.CHALLENGE_RATING] = creatureDef.ChallengeRating;
            parameters[Glossary.Characters.ARMOR_CLASS] = creatureDef.ArmorClass;
            parameters[Glossary.Characters.SPEED] = creatureDef.Speed;

            int con = 0;
            if (creatureDef.Abilities != null)
            {
                foreach (KeyValuePair<string, int> pair in creatureDef.Abilities)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        continue;

                    parameters[pair.Key] = pair.Value;
                    if (pair.Key == Glossary.Characters.CON)
                        con = pair.Value;
                }
            }

            // Without Ancestry/Class, MaxHitPoints formula is (CON)*Level + Bonus.
            // Bake Bonus so GetTotalValue(MaxHitPoints) equals the authored HitPoints.
            int hitPoints = Math.Max(0, creatureDef.HitPoints);
            int maxHpBonus = hitPoints - (con * level);
            parameters[Glossary.Characters.MAX_HIT_POINTS + Glossary.Characters.BONUS_SUFFIX] = maxHpBonus;
            parameters[Glossary.Characters.HIT_POINTS] = hitPoints;

            BakeFinalModifierAsItemsBonus(
                parameters,
                Glossary.Characters.PERCEPTION,
                creatureDef.Perception,
                GetAbility(parameters, Glossary.Characters.WIS));

            if (creatureDef.Saves != null)
            {
                foreach (KeyValuePair<string, int> pair in creatureDef.Saves)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        continue;

                    parameters[pair.Key] = pair.Value;
                }
            }

            if (creatureDef.Skills != null)
            {
                foreach (KeyValuePair<string, int> pair in creatureDef.Skills)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        continue;

                    string abilityKey = ResolveSkillAbilityDependency(pair.Key);
                    int abilityMod = string.IsNullOrEmpty(abilityKey)
                        ? 0
                        : GetAbility(parameters, abilityKey);

                    BakeFinalModifierAsItemsBonus(parameters, pair.Key, pair.Value, abilityMod);
                }
            }

            if (creatureDef.Parameters == null)
                return;

            foreach (KeyValuePair<string, int> pair in creatureDef.Parameters)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                parameters[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Authored totals (Perception / skills) are baked so
        /// <c>GetTotalValue</c> = ability + Untrained(0) + ItemsBonus equals the stat-block number.
        /// </summary>
        private static void BakeFinalModifierAsItemsBonus(
            Dictionary<string, int> parameters,
            string parameterKey,
            int finalModifier,
            int abilityModifier)
        {
            parameters[parameterKey + Glossary.Characters.ITEMS_SUFFIX] = finalModifier - abilityModifier;
        }

        private static int GetAbility(Dictionary<string, int> parameters, string abilityKey)
        {
            return parameters.TryGetValue(abilityKey, out int value) ? value : 0;
        }

        /// <summary>
        /// Minimal skill→ability map aligned with <c>GeneralRule.SkillDependencies</c>
        /// (avoids requiring RuleDef at convert time).
        /// </summary>
        private static string ResolveSkillAbilityDependency(string skillId)
        {
            switch (skillId)
            {
                case Glossary.Characters.ACROBATICS:
                case Glossary.Characters.STEALTH:
                case Glossary.Characters.THIEVERY:
                    return Glossary.Characters.DEX;
                case Glossary.Characters.ARCANA:
                case Glossary.Characters.CRAFTING:
                case Glossary.Characters.LORE:
                case Glossary.Characters.OCCULTISM:
                case Glossary.Characters.SOCIETY:
                    return Glossary.Characters.INT;
                case Glossary.Characters.ATHLETICS:
                    return Glossary.Characters.STR;
                case Glossary.Characters.DECEPTION:
                case Glossary.Characters.DIPLOMACY:
                case Glossary.Characters.INTIMIDATION:
                case Glossary.Characters.PERFORMANCE:
                    return Glossary.Characters.CHA;
                case Glossary.Characters.MEDICINE:
                case Glossary.Characters.NATURE:
                case Glossary.Characters.RELIGION:
                case Glossary.Characters.SURVIVAL:
                case Glossary.Characters.PERCEPTION:
                    return Glossary.Characters.WIS;
                default:
                    return null;
            }
        }

        private static void ApplyWornItemFeatures(
            CharacterStateData character,
            DefinitionsManager definitionsManager)
        {
            if (character.EquippedItems == null || definitionsManager == null)
                return;

            for (int i = 0; i < character.EquippedItems.Count; i++)
            {
                EquippedItemStateData slot = character.EquippedItems[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.ItemId))
                    continue;

                CharacterItemFeaturesOperator.ApplyIfWorn(
                    character,
                    slot.Slot,
                    slot.ItemId,
                    definitionsManager);
            }
        }

        private static Dictionary<string, int> CloneDictionary(Dictionary<string, int> source)
        {
            return source == null
                ? new Dictionary<string, int>()
                : new Dictionary<string, int>(source);
        }

        private static List<EquippedItemStateData> CloneEquippedItems(List<EquippedItemStateData> source)
        {
            if (source == null)
                return new List<EquippedItemStateData>();

            var result = new List<EquippedItemStateData>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                EquippedItemStateData item = source[i];
                if (item == null)
                    continue;

                result.Add(new EquippedItemStateData
                {
                    Slot = item.Slot,
                    ItemId = item.ItemId,
                });
            }

            return result;
        }
    }
}
