using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs.Gems;

namespace Modules.ECS.Scripts.Match3.Systems.Actions
{
    public interface IActionApplier
    {
        IActionApplier Init(EcsWorld world);
        void Apply(MatchAction action);
    }
}
