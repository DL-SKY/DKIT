using Modules.Restrictions.Scripts.Checker;
using Modules.Restrictions.Scripts.Checker.Checkers;
using System;
using System.Collections.Generic;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Restrictions.Scripts.Core
{
    public class RestrictionsChecker
    {
        [Inject] private readonly RestrictionFactory _restrictionFactory;

        private Dictionary<RestrictionType, IChecker> _checkers = new Dictionary<RestrictionType, IChecker>();

        public bool Check(List<Restriction> restrictions)
        {
            foreach (var restriction in restrictions)
                if (!GetChecker(restriction.Type).Check(restriction))
                    return false;

            return true;
        }

        private IChecker GetChecker(RestrictionType type)
        {
            if (!_checkers.ContainsKey(type))
                _checkers.Add(type, CreateChecker(type));

            return _checkers[type];
        }

        private IChecker CreateChecker(RestrictionType type)
        {
            //=>_restrictionFactory.Create<AccountBonusInfoViewModel>();
            //=> _restrictionFactory.Create<AccountPanelViewModel>(new object[] { _currentSocial, _context.State.Player.Id, !_isShowCaution });
            return type switch
            {
                RestrictionType.TimeNow => _restrictionFactory.Create<TimeNowRestrictionChecker>(),

                _ => throw new NotImplementedException()
            };
        }
    }
}
