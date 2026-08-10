using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleActions;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleRules;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items;
using Modules.Dices.Scripts;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.RPG.Scripts.Adventure.CursorBattlePrototype
{
    /// <summary>
    /// Prototype-only combat runtime. Does not alter existing adventure runtime classes.
    /// </summary>
    public sealed class CursorBattlePrototypeService
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;

        private readonly DiceTower _diceTower = new DiceTower();
        private int _nextRuntimeId = -1;

        public CursorBattlePrototypeSession Session { get; private set; }

        public bool IsPlayerTurn()
        {
            return GetCurrentSide()?.IsPlayerControlled ?? false;
        }

        public string GetCurrentSideId()
        {
            return GetCurrentSide()?.Id ?? string.Empty;
        }

        public bool StartDemoEncounter(List<string> creatureIds = null, int actionsPerTurn = 3)
        {
            creatureIds ??= CreateDefaultCreatureList();
            return StartEncounter(creatureIds, actionsPerTurn);
        }

        public bool StartEncounter(List<string> creatureIds, int actionsPerTurn = 3)
        {
            if (Session != null && Session.IsActive)
                return false;

            if (_stateManager?.State?.Characters == null)
                return false;

            var partySide = CreatePartySide();
            if (partySide.Actors.Count == 0)
                return false;

            var enemySide = CreateEnemySide(creatureIds);
            if (enemySide.Actors.Count == 0)
                return false;

            Session = new CursorBattlePrototypeSession
            {
                IsActive = true,
                Round = 1,
                CurrentSideIndex = 0,
                ActionsPerTurn = Math.Max(1, actionsPerTurn),
                RemainingActions = Math.Max(1, actionsPerTurn),
                ResultType = CursorBattlePrototypeResultType.None,
            };

            Session.Sides.Add(partySide);
            Session.Sides.Add(enemySide);
            Log($"Combat started. Party: {partySide.Actors.Count}, enemies: {enemySide.Actors.Count}.");
            return true;
        }

        public List<CursorBattlePrototypeActionPreview> GetAvailableActions(int runtimeActorId)
        {
            var result = new List<CursorBattlePrototypeActionPreview>();
            if (!TryGetActor(runtimeActorId, out var actor))
                return result;

            var battleRule = GetCurrentBattleRule();
            for (int i = 0; i < actor.BattleActionIds.Count; i++)
            {
                string actionId = actor.BattleActionIds[i];
                if (string.IsNullOrWhiteSpace(actionId))
                    continue;

                if (!_definitionsManager.BattleActions.TryGetValue(actionId, out var actionDef) || actionDef == null || actionDef.Disabled)
                    continue;

                int actionCost = Math.Max(1, actionDef.ActionCost);
                bool isAttack = IsAttackAction(actionDef, battleRule);

                result.Add(new CursorBattlePrototypeActionPreview
                {
                    ActionId = actionId,
                    Title = string.IsNullOrWhiteSpace(actionDef.Title) ? actionId : actionDef.Title,
                    ActionCost = actionCost,
                    TargetType = actionDef.TargetType,
                    IsAttack = isAttack,
                });
            }

            return result;
        }

        public List<CursorBattlePrototypeActor> GetTargetCandidates(int runtimeActorId, string actionId)
        {
            var result = new List<CursorBattlePrototypeActor>();
            if (!TryGetActor(runtimeActorId, out var actor))
                return result;

            if (!_definitionsManager.BattleActions.TryGetValue(actionId, out var actionDef) || actionDef == null)
                return result;

            var actorSide = GetSideByActor(actor.RuntimeId);
            var enemySide = GetOppositeSide(actorSide);
            switch (actionDef.TargetType)
            {
                case BattleActionTargetType.Self:
                    if (IsAlive(actor))
                        result.Add(actor);
                    break;
                case BattleActionTargetType.AllySingle:
                case BattleActionTargetType.AllyAll:
                    AddAliveActors(actorSide, result);
                    break;
                case BattleActionTargetType.EnemySingle:
                case BattleActionTargetType.EnemyAll:
                case BattleActionTargetType.RandomEnemy:
                    AddAliveActors(enemySide, result);
                    break;
                default:
                    break;
            }

            return result;
        }

        public bool TryApplyAction(int runtimeActorId, string actionId, int selectedTargetRuntimeId = 0)
        {
            if (Session == null || !Session.IsActive)
                return false;

            if (!TryGetActor(runtimeActorId, out var actor) || !IsAlive(actor))
                return false;

            CursorBattlePrototypeSide side = GetCurrentSide();
            if (side == null || !ContainsActor(side, runtimeActorId))
                return false;

            if (!_definitionsManager.BattleActions.TryGetValue(actionId, out var actionDef) || actionDef == null || actionDef.Disabled)
                return false;

            int actionCost = Math.Max(1, actionDef.ActionCost);
            if (Session.RemainingActions < actionCost)
            {
                Log($"{actor.Character.Name}: not enough actions for '{actionId}'.");
                return false;
            }

            var targets = ResolveTargets(actor, actionDef, selectedTargetRuntimeId);
            if (targets.Count == 0)
            {
                Log($"{actor.Character.Name}: no valid targets for '{actionId}'.");
                return false;
            }

            bool isAttackAction = IsAttackAction(actionDef, GetCurrentBattleRule());
            bool attackHits = true;
            int attackTotal = 0;
            int attackRoll = 0;
            int mapPenalty = 0;

            if (isAttackAction && targets.Count > 0)
            {
                var primaryTarget = targets[0];
                (attackHits, attackRoll, attackTotal, mapPenalty) = ResolveAttack(actor, primaryTarget, actionDef);
            }

            ApplyEffects(actor, actionDef, targets, isAttackAction, attackHits);

            Session.RemainingActions -= actionCost;
            if (isAttackAction)
                IncrementAttackCounter(actor.RuntimeId);

            if (isAttackAction)
            {
                string targetName = targets.Count > 0 ? targets[0].Character.Name : "unknown";
                Log($"{actor.Character.Name} uses {actionDef.Id} on {targetName}: roll {attackRoll}, total {attackTotal}, MAP {mapPenalty}, hit={attackHits}.");
            }
            else
            {
                Log($"{actor.Character.Name} uses {actionDef.Id}.");
            }

            EvaluateVictory();
            if (!Session.IsActive)
                return true;

            if (Session.RemainingActions <= 0)
                EndTurn();

            return true;
        }

        public void RunAiIfNeeded()
        {
            if (Session == null || !Session.IsActive)
                return;

            int guard = 0;
            while (Session.IsActive && GetCurrentSide() != null && !GetCurrentSide().IsPlayerControlled && guard < 32)
            {
                guard++;
                if (!TryRunAiStep())
                    EndTurn();
            }
        }

        public void EndTurn()
        {
            if (Session == null || !Session.IsActive)
                return;

            Session.CurrentSideIndex = (Session.CurrentSideIndex + 1) % Session.Sides.Count;
            if (Session.CurrentSideIndex == 0)
                Session.Round++;

            Session.RemainingActions = Session.ActionsPerTurn;
            Log($"Turn changed. Side: {GetCurrentSide()?.Id}, round: {Session.Round}.");
        }

        public void CommitPartyToState()
        {
            if (Session == null)
                return;

            CursorBattlePrototypeSide party = Session.Sides.Count > 0 ? Session.Sides[0] : null;
            if (party == null)
                return;

            foreach (CursorBattlePrototypeActor actor in party.Actors)
            {
                if (actor.SourceCharacterId <= 0 || actor.Character == null)
                    continue;

                var request = new UpdateCharacterRequestData
                {
                    CharacterId = actor.SourceCharacterId,
                    CharacterData = CharacterParametersOperator.CreateCharacterRequestSnapshot(actor.Character),
                };

                _stateLogic.ProcessAction(new UpdateCharacterStateAction(request));
            }
        }

        public void FinishAndClose(CursorBattlePrototypeResultType resultType)
        {
            if (Session == null)
                return;

            Session.ResultType = resultType;
            Session.IsActive = false;
            CommitPartyToState();
            Log($"Combat finished: {resultType}.");
        }

        private CursorBattlePrototypeSide CreatePartySide()
        {
            var side = new CursorBattlePrototypeSide
            {
                Id = "Party",
                IsPlayerControlled = true,
            };

            List<int> activePartyIds = _stateManager.State.Characters.ActivePartyCharacterIds;
            Dictionary<int, CharacterStateData> roster = _stateManager.State.Characters.Characters;
            if (activePartyIds == null || roster == null)
                return side;

            for (int i = 0; i < activePartyIds.Count; i++)
            {
                int sourceCharacterId = activePartyIds[i];
                if (!roster.TryGetValue(sourceCharacterId, out CharacterStateData sourceCharacter) || sourceCharacter == null)
                    continue;

                CharacterStateData clone = CreatureCombatantFactory.CloneForBattle(sourceCharacter);
                var actor = new CursorBattlePrototypeActor
                {
                    RuntimeId = sourceCharacterId,
                    SourceCharacterId = sourceCharacterId,
                    IsPlayerSide = true,
                    Character = clone,
                };
                actor.BattleActionIds = ResolveActorActionIds(actor, null);
                side.Actors.Add(actor);
            }

            return side;
        }

        private CursorBattlePrototypeSide CreateEnemySide(List<string> creatureIds)
        {
            var side = new CursorBattlePrototypeSide
            {
                Id = "Enemies",
                IsPlayerControlled = false,
            };

            if (creatureIds == null || creatureIds.Count == 0 || _definitionsManager?.Creatures == null)
                return side;

            for (int i = 0; i < creatureIds.Count; i++)
            {
                string creatureId = creatureIds[i];
                if (string.IsNullOrWhiteSpace(creatureId))
                    continue;

                if (!_definitionsManager.Creatures.TryGetValue(creatureId, out var creatureDef) || creatureDef == null || creatureDef.Disabled)
                    continue;

                CharacterStateData combatant = CreatureCombatantFactory.CreateFromCreature(
                    creatureDef,
                    _nextRuntimeId--,
                    _definitionsManager);
                if (combatant == null)
                    continue;

                var actor = new CursorBattlePrototypeActor
                {
                    RuntimeId = combatant.Id,
                    SourceCharacterId = 0,
                    SourceCreatureId = creatureDef.Id,
                    IsPlayerSide = false,
                    Character = combatant,
                };
                actor.BattleActionIds = ResolveActorActionIds(actor, creatureDef.BattleActionIds);
                side.Actors.Add(actor);
            }

            return side;
        }

        private List<string> ResolveActorActionIds(CursorBattlePrototypeActor actor, List<string> sourceActionIds)
        {
            var result = new List<string>();
            if (sourceActionIds != null)
            {
                for (int i = 0; i < sourceActionIds.Count; i++)
                {
                    string id = sourceActionIds[i];
                    if (string.IsNullOrWhiteSpace(id))
                        continue;
                    if (!_definitionsManager.BattleActions.ContainsKey(id))
                        continue;
                    result.Add(id);
                }
            }

            if (result.Count == 0 && _definitionsManager.BattleActions.ContainsKey("_Strike"))
                result.Add("_Strike");

            return result;
        }

        private List<CursorBattlePrototypeActor> ResolveTargets(
            CursorBattlePrototypeActor actor,
            BattleActionDef actionDef,
            int selectedTargetRuntimeId)
        {
            var targets = new List<CursorBattlePrototypeActor>();
            CursorBattlePrototypeSide actorSide = GetSideByActor(actor.RuntimeId);
            CursorBattlePrototypeSide enemySide = GetOppositeSide(actorSide);

            switch (actionDef.TargetType)
            {
                case BattleActionTargetType.Self:
                    targets.Add(actor);
                    break;

                case BattleActionTargetType.AllySingle:
                    TryAddSingleTarget(actorSide, selectedTargetRuntimeId, actor, targets);
                    break;

                case BattleActionTargetType.AllyAll:
                    AddAliveActors(actorSide, targets);
                    break;

                case BattleActionTargetType.EnemySingle:
                    TryAddSingleTarget(enemySide, selectedTargetRuntimeId, null, targets);
                    break;

                case BattleActionTargetType.EnemyAll:
                    AddAliveActors(enemySide, targets);
                    break;

                case BattleActionTargetType.RandomEnemy:
                    var allEnemies = new List<CursorBattlePrototypeActor>();
                    AddAliveActors(enemySide, allEnemies);
                    if (allEnemies.Count > 0)
                    {
                        int index = UnityEngine.Random.Range(0, allEnemies.Count);
                        targets.Add(allEnemies[index]);
                    }
                    break;
            }

            return targets;
        }

        private void TryAddSingleTarget(
            CursorBattlePrototypeSide side,
            int selectedTargetRuntimeId,
            CursorBattlePrototypeActor fallback,
            List<CursorBattlePrototypeActor> targets)
        {
            if (side == null || targets == null)
                return;

            if (selectedTargetRuntimeId != 0 && TryGetActor(selectedTargetRuntimeId, out var explicitTarget) && IsAlive(explicitTarget))
            {
                if (ContainsActor(side, explicitTarget.RuntimeId))
                {
                    targets.Add(explicitTarget);
                    return;
                }
            }

            CursorBattlePrototypeActor firstAlive = FindFirstAlive(side);
            if (firstAlive != null)
            {
                targets.Add(firstAlive);
                return;
            }

            if (fallback != null && IsAlive(fallback))
                targets.Add(fallback);
        }

        private (bool hit, int attackRoll, int attackTotal, int mapPenalty) ResolveAttack(
            CursorBattlePrototypeActor actor,
            CursorBattlePrototypeActor target,
            BattleActionDef actionDef)
        {
            ItemDef weapon = FindMainWeapon(actor.Character);
            var weaponProxy = new WeaponProxy(actor.Character);
            int attackModifier = weaponProxy.GetAttackModifier(weapon);
            int mapPenalty = GetMapPenalty(actor.RuntimeId, actionDef, weapon);

            DiceResult attackRoll = _diceTower.Roll(DiceType.D20);
            int attackTotal = attackRoll.Result + attackModifier + mapPenalty;

            int targetArmorClass = 10;
            if (target?.Character?.Parameters != null
                && target.Character.Parameters.TryGetValue(Glossary.Characters.ARMOR_CLASS, out int ac))
            {
                targetArmorClass = ac;
            }

            bool isCriticalFailure = attackRoll.Type == DiceResultType.CriticalFailure;
            bool isCriticalHit = attackRoll.Type == DiceResultType.CriticalHit;
            bool hit = isCriticalHit || (!isCriticalFailure && attackTotal >= targetArmorClass);
            return (hit, attackRoll.Result, attackTotal, mapPenalty);
        }

        private void ApplyEffects(
            CursorBattlePrototypeActor actor,
            BattleActionDef actionDef,
            List<CursorBattlePrototypeActor> targets,
            bool isAttackAction,
            bool attackHits)
        {
            if (actionDef.Effects == null || actionDef.Effects.Count == 0)
                return;

            for (int i = 0; i < actionDef.Effects.Count; i++)
            {
                BattleActionEffectData effect = actionDef.Effects[i];
                if (effect == null)
                    continue;

                if (effect.OnHitOnly && isAttackAction && !attackHits)
                    continue;

                for (int t = 0; t < targets.Count; t++)
                    ApplyEffectToTarget(actor, targets[t], effect);
            }
        }

        private void ApplyEffectToTarget(
            CursorBattlePrototypeActor actor,
            CursorBattlePrototypeActor target,
            BattleActionEffectData effect)
        {
            if (target?.Character == null || effect == null)
                return;

            target.Character.Parameters ??= new Dictionary<string, int>();

            switch (effect.Type)
            {
                case BattleActionEffectType.Damage:
                {
                    int amount = ResolveDamageValue(actor, effect);
                    ChangeHitPoints(target.Character, -Math.Abs(amount));
                    break;
                }

                case BattleActionEffectType.Heal:
                {
                    int amount = Math.Abs(effect.Value);
                    ChangeHitPoints(target.Character, amount);
                    break;
                }

                case BattleActionEffectType.AddStatus:
                {
                    if (!string.IsNullOrWhiteSpace(effect.StatusId))
                        CharacterParametersOperator.ApplyFeat(target.Character, effect.StatusId, _definitionsManager);
                    break;
                }

                case BattleActionEffectType.RemoveStatus:
                {
                    if (!string.IsNullOrWhiteSpace(effect.StatusId))
                        CharacterParametersOperator.UnapplyFeat(target.Character, effect.StatusId, _definitionsManager);
                    break;
                }

                case BattleActionEffectType.ModifyParam:
                {
                    if (!string.IsNullOrWhiteSpace(effect.StatusId))
                    {
                        target.Character.Parameters.TryGetValue(effect.StatusId, out int current);
                        target.Character.Parameters[effect.StatusId] = current + effect.Value;
                    }
                    break;
                }
            }
        }

        private int ResolveDamageValue(CursorBattlePrototypeActor actor, BattleActionEffectData effect)
        {
            if (effect.Value != 0)
                return Math.Abs(effect.Value);

            ItemDef weapon = FindMainWeapon(actor.Character);
            if (weapon == null)
                return 1;

            var weaponProxy = new WeaponProxy(actor.Character);
            int total = 0;
            IReadOnlyList<WeaponDamageDicePartData> diceParts = weaponProxy.GetDamageDice(weapon);
            for (int i = 0; i < diceParts.Count; i++)
            {
                WeaponDamageDicePartData part = diceParts[i];
                if (part == null)
                    continue;

                int count = Math.Max(1, part.Count);
                total += _diceTower.Roll(part.DiceType, count, 0).Result;
            }

            total += weaponProxy.GetDamageModifier(weapon);
            return Math.Max(1, total);
        }

        private void ChangeHitPoints(CharacterStateData character, int delta)
        {
            character.Parameters ??= new Dictionary<string, int>();
            character.Parameters.TryGetValue(Glossary.Characters.HIT_POINTS, out int hp);
            int nextHp = hp + delta;
            if (nextHp < 0)
                nextHp = 0;

            character.Parameters[Glossary.Characters.HIT_POINTS] = nextHp;
            character.IsDead = nextHp <= 0;
            if (character.IsDead)
                character.DeathTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private int GetMapPenalty(int runtimeActorId, BattleActionDef actionDef, ItemDef weapon)
        {
            BattleRuleDef rule = GetCurrentBattleRule();
            if (rule == null || !rule.UseMultipleAttackPenalty || !IsAttackAction(actionDef, rule))
                return 0;

            Session.AttackCounterPerActor.TryGetValue(runtimeActorId, out int attackCount);
            int attackNumber = attackCount + 1;
            List<int> table = GetMapTable(rule, weapon);
            if (table == null || table.Count == 0)
                return 0;

            int index = attackNumber - 1;
            if (index < 0)
                index = 0;
            if (index >= table.Count)
                index = table.Count - 1;
            return table[index];
        }

        private List<int> GetMapTable(BattleRuleDef rule, ItemDef weapon)
        {
            if (rule == null)
                return null;

            if (weapon?.Tags != null && rule.MultipleAttackPenaltyTableByWeaponTag != null)
            {
                for (int i = 0; i < weapon.Tags.Count; i++)
                {
                    string tag = weapon.Tags[i];
                    if (string.IsNullOrWhiteSpace(tag))
                        continue;

                    if (rule.MultipleAttackPenaltyTableByWeaponTag.TryGetValue(tag, out List<int> tableByTag)
                        && tableByTag != null
                        && tableByTag.Count > 0)
                    {
                        return tableByTag;
                    }
                }
            }

            return rule.MultipleAttackPenaltyTable;
        }

        private bool IsAttackAction(BattleActionDef actionDef, BattleRuleDef battleRule)
        {
            if (actionDef?.Tags == null || battleRule?.AttackTags == null)
                return false;

            for (int i = 0; i < actionDef.Tags.Count; i++)
            {
                string actionTag = actionDef.Tags[i];
                if (string.IsNullOrWhiteSpace(actionTag))
                    continue;

                for (int j = 0; j < battleRule.AttackTags.Count; j++)
                {
                    if (string.Equals(actionTag, battleRule.AttackTags[j], StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        private BattleRuleDef GetCurrentBattleRule()
        {
            if (_definitionsManager?.RuleSettings == null || _definitionsManager.BattleRules == null)
                return null;

            string ruleId = _definitionsManager.RuleSettings.BattleRule;
            if (string.IsNullOrWhiteSpace(ruleId))
                return null;

            if (_definitionsManager.BattleRules.TryGetValue(ruleId, out BattleRuleDef rule))
                return rule;

            return null;
        }

        private bool TryRunAiStep()
        {
            CursorBattlePrototypeSide aiSide = GetCurrentSide();
            CursorBattlePrototypeSide playerSide = GetOppositeSide(aiSide);
            if (aiSide == null || playerSide == null)
                return false;

            CursorBattlePrototypeActor actor = FindFirstAlive(aiSide);
            CursorBattlePrototypeActor target = FindFirstAlive(playerSide);
            if (actor == null || target == null)
                return false;

            List<CursorBattlePrototypeActionPreview> actions = GetAvailableActions(actor.RuntimeId);
            if (actions.Count == 0)
                return false;

            CursorBattlePrototypeActionPreview selectedAction = actions[0];
            return TryApplyAction(actor.RuntimeId, selectedAction.ActionId, target.RuntimeId);
        }

        private void EvaluateVictory()
        {
            CursorBattlePrototypeSide partySide = Session.Sides[0];
            CursorBattlePrototypeSide enemySide = Session.Sides[1];

            bool partyAlive = FindFirstAlive(partySide) != null;
            bool enemyAlive = FindFirstAlive(enemySide) != null;

            if (!enemyAlive)
            {
                Session.ResultType = CursorBattlePrototypeResultType.Victory;
                Session.IsActive = false;
                CommitPartyToState();
                Log("Party wins.");
                return;
            }

            if (!partyAlive)
            {
                Session.ResultType = CursorBattlePrototypeResultType.Defeat;
                Session.IsActive = false;
                CommitPartyToState();
                Log("Party is defeated.");
            }
        }

        private ItemDef FindMainWeapon(CharacterStateData character)
        {
            if (character?.EquippedItems == null || _definitionsManager?.Items == null)
                return null;

            for (int i = 0; i < character.EquippedItems.Count; i++)
            {
                EquippedItemStateData slot = character.EquippedItems[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.ItemId))
                    continue;

                if (!_definitionsManager.Items.TryGetValue(slot.ItemId, out ItemDef itemDef) || itemDef == null)
                    continue;

                if (itemDef.Category == ItemCategory.Weapon)
                    return itemDef;
            }

            return null;
        }

        private void IncrementAttackCounter(int runtimeActorId)
        {
            Session.AttackCounterPerActor.TryGetValue(runtimeActorId, out int count);
            Session.AttackCounterPerActor[runtimeActorId] = count + 1;
        }

        private CursorBattlePrototypeSide GetCurrentSide()
        {
            if (Session == null || Session.Sides == null || Session.Sides.Count == 0)
                return null;

            if (Session.CurrentSideIndex < 0 || Session.CurrentSideIndex >= Session.Sides.Count)
                return null;

            return Session.Sides[Session.CurrentSideIndex];
        }

        private CursorBattlePrototypeSide GetSideByActor(int runtimeActorId)
        {
            if (Session?.Sides == null)
                return null;

            for (int i = 0; i < Session.Sides.Count; i++)
            {
                if (ContainsActor(Session.Sides[i], runtimeActorId))
                    return Session.Sides[i];
            }

            return null;
        }

        private CursorBattlePrototypeSide GetOppositeSide(CursorBattlePrototypeSide side)
        {
            if (Session?.Sides == null || Session.Sides.Count < 2 || side == null)
                return null;

            if (ReferenceEquals(Session.Sides[0], side))
                return Session.Sides[1];

            if (ReferenceEquals(Session.Sides[1], side))
                return Session.Sides[0];

            return null;
        }

        private bool TryGetActor(int runtimeActorId, out CursorBattlePrototypeActor actor)
        {
            actor = null;
            if (Session?.Sides == null)
                return false;

            for (int i = 0; i < Session.Sides.Count; i++)
            {
                CursorBattlePrototypeSide side = Session.Sides[i];
                if (side?.Actors == null)
                    continue;

                for (int j = 0; j < side.Actors.Count; j++)
                {
                    CursorBattlePrototypeActor candidate = side.Actors[j];
                    if (candidate != null && candidate.RuntimeId == runtimeActorId)
                    {
                        actor = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static void AddAliveActors(CursorBattlePrototypeSide side, List<CursorBattlePrototypeActor> result)
        {
            if (side?.Actors == null || result == null)
                return;

            for (int i = 0; i < side.Actors.Count; i++)
            {
                CursorBattlePrototypeActor actor = side.Actors[i];
                if (IsAlive(actor))
                    result.Add(actor);
            }
        }

        private static CursorBattlePrototypeActor FindFirstAlive(CursorBattlePrototypeSide side)
        {
            if (side?.Actors == null)
                return null;

            for (int i = 0; i < side.Actors.Count; i++)
            {
                CursorBattlePrototypeActor actor = side.Actors[i];
                if (IsAlive(actor))
                    return actor;
            }

            return null;
        }

        private static bool ContainsActor(CursorBattlePrototypeSide side, int runtimeActorId)
        {
            if (side?.Actors == null)
                return false;

            for (int i = 0; i < side.Actors.Count; i++)
            {
                if (side.Actors[i] != null && side.Actors[i].RuntimeId == runtimeActorId)
                    return true;
            }

            return false;
        }

        private static bool IsAlive(CursorBattlePrototypeActor actor)
        {
            return actor != null && actor.Character != null && !actor.Character.IsDead;
        }

        private void Log(string message)
        {
            if (Session == null)
                return;

            Session.Log.Add(new CursorBattlePrototypeLogEntry
            {
                TimeUtc = DateTime.UtcNow,
                Message = message ?? string.Empty,
            });
        }

        private List<string> CreateDefaultCreatureList()
        {
            var result = new List<string>();
            if (_definitionsManager?.Creatures == null)
                return result;

            if (_definitionsManager.Creatures.ContainsKey("_GoblinWarrior"))
                result.Add("_GoblinWarrior");
            if (_definitionsManager.Creatures.ContainsKey("_Wolf"))
                result.Add("_Wolf");

            if (result.Count > 0)
                return result;

            var available = new List<string>();
            foreach (var pair in _definitionsManager.Creatures)
            {
                if (pair.Value == null || pair.Value.Disabled)
                    continue;
                available.Add(pair.Key);
            }

            if (available.Count == 0)
                return result;

            int partyCount = _stateManager?.State?.Characters?.ActivePartyCharacterIds?.Count ?? 1;
            int targetEnemyCount = UnityEngine.Mathf.Clamp(partyCount, 1, 4);
            while (result.Count < targetEnemyCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, available.Count);
                result.Add(available[randomIndex]);
            }

            return result;
        }
    }
}
