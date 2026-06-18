using Modules.Definitions.Scripts.Implementation.Defs.Objectives;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Interfaces;
using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.Match3.Scripts.Implementation.Data
{
    public class ObjectivesData : IObjectivesData
    {
        private int _startTurnsCount;
        private List<ScoreData> _startScoreValues;
        private List<Restriction> _victoryConditions;
        private List<Restriction> _defeatConditions;

        public ObjectivesData(ObjectivesDef objectivesDef)
        {
            //TODO: ...
            // Подсчитать реальное значение стартовой величины Ходов
            _startTurnsCount = 0;

            // Стартовое значение очков
            _startScoreValues = new List<ScoreData>();
            foreach (var score in objectivesDef.StartScores)
                _startScoreValues.Add(score);

            // Данные об условиях окончания игры
            _victoryConditions = objectivesDef.VictoryConditions;
            _defeatConditions = objectivesDef.DefeatConditions;
        }

        public int GetTurnsCount()
        {
            return _startTurnsCount;
        }

        public List<ScoreData> GetStartScoreValues()
        {
            return _startScoreValues;
        }

        public List<Restriction> GetVictoryConditions()
        {
            return _victoryConditions;
        }

        public List<Restriction> GetDefeatConditions()
        {
            return _defeatConditions;
        }
    }
}
