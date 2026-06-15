using Modules.Restrictions.Scripts.Checker;

namespace Zenject.Scripts.Factories
{
    public class RestrictionFactory
    {
        [Inject] private readonly DiContainer _container;

        public T Create<T>(params object[] parameters) where T : IChecker
        {
            var checker = _container.Instantiate<T>(parameters);
            return checker;
        }
    }
}
