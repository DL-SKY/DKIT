using Modules.Match3.Scripts.Interfaces;
using System;

namespace Modules.Match3.Scripts.Core
{
    public abstract class RoundControllerBase : IDisposable
    {
        public Action<RoundStateType> OnStateChange;

        public RoundStateType StateType => _stateType;
        protected RoundStateType _stateType;

        protected IGameZoneData _gameZoneData;
        protected IGemsData _gemsData;
        protected IObjectivesData _objectivesData;

        protected void InitBase(IGameZoneData gameZoneData, IGemsData gemsData, IObjectivesData objectivesData)
        {
            _gameZoneData = gameZoneData;
            _gemsData = gemsData;
            _objectivesData = objectivesData;

            Subscribe();
            InitImplementation();
        }

        public abstract void Dispose();

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract void InitImplementation();
    }
}
