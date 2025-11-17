using Leopotam.Ecs;
using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Match3.Scripts.Interfaces;
using System;
using Zenject;

namespace Modules.ECS.Scripts.Match3.Systems
{
    public class GemsInitSystem : IEcsInitSystem
    {
        private const string GEM = "Prefabs/Gems/Gem";

        [Inject] private readonly DefinitionsManager _definitionsManager;

        private readonly EcsWorld _world = null;
        private readonly IGameRoundData _gameRoundData;

        public GemsInitSystem(IGameRoundData gameRoundData)
        {
            _gameRoundData = gameRoundData;
        }

        public void Init()
        {
            throw new NotImplementedException();
        }
    }
}
