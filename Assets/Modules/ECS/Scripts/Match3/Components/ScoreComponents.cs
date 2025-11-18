namespace Modules.ECS.Scripts.Match3.Components
{
    public struct MatchScoreRequest
    {
        /// <summary>
        /// Количество фишек в группе (3, 4, 5+)
        /// </summary>
        public int Count;

        /// <summary>
        /// Тип фишки (DefId), которая совпала
        /// </summary>
        public string GemType;
    }
}
