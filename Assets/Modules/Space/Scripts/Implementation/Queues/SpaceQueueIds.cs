namespace Modules.Space.Scripts.Implementation.Queues
{
    public static class SpaceQueueIds
    {
        public const string SKILL = "skill";
        public const string BLUEPRINT_IMPROVEMENT = "blueprint_improvement";
        public const string DISASSEMBLY = "disassembly";
        public const string BLUEPRINT_ATTEMPT = "blueprint_attempt";
        public const string PRODUCTION = "production";

        public const int FREE_MAX_COUNT = 1;

        public static readonly string[] ALL =
        {
            SKILL,
            BLUEPRINT_IMPROVEMENT,
            DISASSEMBLY,
            BLUEPRINT_ATTEMPT,
            PRODUCTION
        };

        public static bool TryGetKind(string queueId, out SpaceQueueKind kind)
        {
            switch (queueId)
            {
                case SKILL:
                    kind = SpaceQueueKind.Skill;
                    return true;
                case BLUEPRINT_IMPROVEMENT:
                    kind = SpaceQueueKind.BlueprintImprovement;
                    return true;
                case DISASSEMBLY:
                    kind = SpaceQueueKind.Disassembly;
                    return true;
                case BLUEPRINT_ATTEMPT:
                    kind = SpaceQueueKind.BlueprintAttempt;
                    return true;
                case PRODUCTION:
                    kind = SpaceQueueKind.Production;
                    return true;
                default:
                    kind = default;
                    return false;
            }
        }
    }
}
