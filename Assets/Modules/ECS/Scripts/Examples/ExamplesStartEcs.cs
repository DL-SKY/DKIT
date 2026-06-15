using Leopotam.Ecs;
using UnityEngine;

namespace Modules.ECS.Scripts.Examples
{
    public class ExamplesStartEcs : MonoBehaviour
    {
        private EcsWorld _world;
        private EcsSystems _systems;

        void Start()
        {
            _world = new EcsWorld();
            _systems = new EcsSystems(_world);

            _systems
                //.Add(new GridInitSystem())
                //.Add(new SelectionSystem())
                //.Add(new SwapSystem())
                //.Add(new MatchCheckSystem())
                //.Add(new DestroySystem())

                .Add(new GridInitSystem())
                .Add(new DragStartSystem())
                .Add(new DragEndSystem())
                .Add(new DragAnimationSystem()) // Опционально
                .Add(new SwapSystem())
                .Add(new MatchCheckSystem())
                .Add(new DestroySystem())

                .Init();
        }

        void Update()
        {
            _systems?.Run();
        }

        void OnDestroy()
        {
            _systems?.Destroy();
            _systems = null;
            _world?.Destroy();
            _world = null;
        }
    }
}
