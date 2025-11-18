using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.ECS.Scripts.Match3.Components;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems.Settings
{
    /// <summary>
    /// Система хранения глобальных настроек
    /// </summary>
    public class Match3GlobalSettingsSystem : IEcsInitSystem
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;

        public void Init()
        {
            var settings = _definitionsManager.Match3GlobalSettings;

            var entity = _world.NewEntity();
            entity.Get<Match3GlobalSettingsData>() = new Match3GlobalSettingsData
            {
                SwapAnimationDurationMs = settings.SwapAnimationDurationMs
            };
        }
    }
}
