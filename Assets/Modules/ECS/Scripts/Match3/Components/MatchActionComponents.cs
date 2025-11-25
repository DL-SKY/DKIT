using Modules.Definitions.Scripts.Implementation.Defs.Gems;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент-запрос на выполнение экшена после совпадения фишек (применние match-механики)
    /// </summary>
    public struct MatchActionRequest
    {
        public MatchAction Action;
    }
}
