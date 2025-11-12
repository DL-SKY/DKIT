using Modules.Restrictions.Scripts.Checker;
using Modules.Restrictions.Scripts.Checker.Checkers;
using Modules.Restrictions.Scripts.Core;
using Modules.Utils.Scripts.Components;
using System.Collections.Generic;
using Zenject;

namespace Modules.Restrictions.Scripts.Implementation.Checker
{
    public class CheckerContext : ICheckerContext
    {
        [Inject] private readonly Updater _updater;

        private Dictionary<RestrictionType, object> _contexts = new Dictionary<RestrictionType, object>();

        public CheckerContext()
        {
            FillContext();
        }

        public T GetContext<T>(RestrictionType type)
        {
            if (_contexts.TryGetValue(type, out var data))
                if (data is T)
                    return (T)data;

            return default;
        }

        private void FillContext()
        {
            _contexts.Add(RestrictionType.TimeNowRestriction, TimeNowRestrictionChecker.GetContextData());
        }
    }
}
