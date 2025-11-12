using Leopotam.Ecs;

namespace Zenject.Scripts.Factories
{
    public class EcsSystemFactory
    {
        [Inject] private readonly DiContainer _container;

        public T Create<T>(params object[] parameters) where T : IEcsSystem
        {
            var system = _container.Instantiate<T>(parameters);
            return system;
        }
    }
}
