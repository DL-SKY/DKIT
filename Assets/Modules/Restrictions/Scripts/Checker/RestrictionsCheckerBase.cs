using Modules.Restrictions.Scripts.Checker.Checkers;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Restrictions.Scripts.Checker
{
    public abstract class RestrictionsCheckerBase
    {
        protected Dictionary<RestrictionType, IRestrictionChecker> _checkers;

        private readonly ICheckerContext _context;

        protected RestrictionsCheckerBase()
        {
            _checkers = new Dictionary<RestrictionType, IRestrictionChecker>
            {
                //TODO: ...

                { RestrictionType.TimeNowRestriction, new TimeNowRestrictionChecker() },
            };
            FillCheckers();

            _context = CreateContext();
        }

        public bool Check(List<Restriction> restrictions)
        {
            foreach (var restriction in restrictions)
            {
                if (_checkers.TryGetValue(restriction.Type, out var checker))
                {
                    if (!checker.Check(_context, restriction))
                    {
                        return false;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"[RestrictionsCheckerBase] :: Not found checker for type {restriction.Type}!");
                    return false;
                }
            }

            return true;
        }

        protected abstract void FillCheckers();
        protected abstract ICheckerContext CreateContext();
    }
}
