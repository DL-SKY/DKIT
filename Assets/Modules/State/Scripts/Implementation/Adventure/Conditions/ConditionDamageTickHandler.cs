using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Conditions;
using Modules.Dices.Scripts;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Conditions
{
    public class ConditionDamageTickHandler : IConditionTickHandler
    {
        private static readonly DiceTower DICE_TOWER = new DiceTower();

        public bool CanHandle(ConditionDef conditionDef)
        {
            return conditionDef?.Tick != null
                && (conditionDef.Tick.FlatDamage > 0 || HasDamageDice(conditionDef.Tick.DamageDice));
        }

        public void ApplyTick(ConditionTickContext context, ConditionDef conditionDef)
        {
            CharacterStateData character = context?.Character;
            if (character?.Parameters == null || conditionDef?.Tick == null)
                return;

            int damage = conditionDef.Tick.FlatDamage + RollDamageDice(conditionDef.Tick.DamageDice);
            if (damage <= 0)
                return;

            character.Parameters.TryGetValue(Glossary.Characters.HIT_POINTS, out int currentHitPoints);
            int nextHitPoints = currentHitPoints - damage;
            character.Parameters[Glossary.Characters.HIT_POINTS] = nextHitPoints > 0 ? nextHitPoints : 0;
        }

        private static int RollDamageDice(List<ConditionDamageDicePartData> damageDice)
        {
            if (!HasDamageDice(damageDice))
                return 0;

            int total = 0;
            for (int i = 0; i < damageDice.Count; i++)
            {
                ConditionDamageDicePartData part = damageDice[i];
                if (part == null || part.Count <= 0)
                    continue;

                DiceResult result = DICE_TOWER.Roll(part.DiceType, part.Count, 0);
                total += result.Result;
            }

            return total;
        }

        private static bool HasDamageDice(List<ConditionDamageDicePartData> damageDice)
        {
            if (damageDice == null || damageDice.Count == 0)
                return false;

            for (int i = 0; i < damageDice.Count; i++)
            {
                ConditionDamageDicePartData part = damageDice[i];
                if (part != null && part.Count > 0)
                    return true;
            }

            return false;
        }
    }
}
