using Modules.State.Scripts.Implementation.Adventure.StateDatas;

namespace Modules.State.Scripts.Implementation.Adventure
{
    /// <summary>
    /// Shared Pathfinder-style proficiency bonus: Untrained = 0, otherwise level + rank bonus.
    /// </summary>
    public static class ProficiencyBonus
    {
        public static bool TryParseRank(int rankValue, out ProficiencyType rank)
        {
            if (rankValue < (int)ProficiencyType.Untrained || rankValue > (int)ProficiencyType.Legendary)
            {
                rank = ProficiencyType.Untrained;
                return false;
            }

            rank = (ProficiencyType)rankValue;
            return true;
        }

        public static int GetRankBonus(ProficiencyType rank)
        {
            return rank switch
            {
                ProficiencyType.Trained => 2,
                ProficiencyType.Expert => 4,
                ProficiencyType.Master => 6,
                ProficiencyType.Legendary => 8,
                _ => 0,
            };
        }

        /// <summary>
        /// Returns 0 for Untrained / invalid rank; otherwise <paramref name="level"/> + rank bonus.
        /// </summary>
        public static int Evaluate(int rankValue, int level)
        {
            if (!TryParseRank(rankValue, out ProficiencyType rank) || rank == ProficiencyType.Untrained)
                return 0;

            return level + GetRankBonus(rank);
        }
    }
}
