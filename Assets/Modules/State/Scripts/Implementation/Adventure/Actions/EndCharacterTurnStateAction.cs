using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Conditions;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    /// <summary>
    /// Decrements timed condition effects on character turn end.
    /// When timer reaches zero, the condition feat is unapplied.
    /// </summary>
    /// <remarks>
    /// Счётчики timed Condition уменьшаются только здесь.
    /// Дополнительная логика тика берётся из ConditionDef (по id, совпадающему с FeatDef id)
    /// через ConditionDefTickProcessor.
    /// </remarks>
    public class EndCharacterTurnStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.Characters;

        private readonly EndCharacterTurnRequestData _request;
        private readonly DefinitionsManager _definitionsManager;

        public EndCharacterTurnStateAction(
            EndCharacterTurnRequestData request,
            DefinitionsManager definitionsManager)
        {
            _request = request;
            _definitionsManager = definitionsManager;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Characters?.Characters == null)
                return StateActionValidationResult.Fail("Characters dictionary is null.", 101);

            if (_request == null)
                return StateActionValidationResult.Fail("End character turn request is null.", 102);

            if (_request.CharacterId <= 0)
                return StateActionValidationResult.Fail("Character id must be greater than zero.", 103);

            if (!state.Characters.Characters.TryGetValue(_request.CharacterId, out CharacterStateData character) || character == null)
                return StateActionValidationResult.Fail("Character is not found by id.", 104);

            if (_definitionsManager?.Feats == null)
                return StateActionValidationResult.Fail("DefinitionsManager feats collection is null.", 105);

            if (_definitionsManager.Conditions == null)
                return StateActionValidationResult.Fail("DefinitionsManager conditions collection is null.", 107);

            if (character.Parameters == null)
                return StateActionValidationResult.Fail("Character parameters are null.", 106);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            CharacterStateData character = state.Characters.Characters[_request.CharacterId];
            character.StatusEffects ??= new Dictionary<string, int>();

            if (character.StatusEffects.Count == 0)
                return;

            var expiredConditionFeatIds = new List<string>();
            var effectsSnapshot = new List<KeyValuePair<string, int>>(character.StatusEffects);
            for (int i = 0; i < effectsSnapshot.Count; i++)
            {
                KeyValuePair<string, int> pair = effectsSnapshot[i];
                if (!TryGetTimedConditionFeat(pair.Key))
                    continue;

                ConditionDefTickProcessor.ApplyTurnTick(character, pair.Key, _definitionsManager);

                int remainingTurns = pair.Value - 1;
                if (remainingTurns <= 0)
                {
                    expiredConditionFeatIds.Add(pair.Key);
                    continue;
                }

                character.StatusEffects[pair.Key] = remainingTurns;
            }

            for (int i = 0; i < expiredConditionFeatIds.Count; i++)
            {
                CharacterParametersOperator.UnapplyFeat(
                    character,
                    state?.Inventory?.Items,
                    expiredConditionFeatIds[i],
                    _definitionsManager);
            }
        }

        private bool TryGetTimedConditionFeat(string featId)
        {
            if (string.IsNullOrWhiteSpace(featId))
                return false;

            FeatDef featDef;
            if (!_definitionsManager.Feats.TryGetValue(featId, out featDef) || featDef == null)
                return false;

            return featDef.Type == FeatType.Condition
                && featDef.Apply != null
                && featDef.Apply.ConditionDuration > 0;
        }
    }
}
