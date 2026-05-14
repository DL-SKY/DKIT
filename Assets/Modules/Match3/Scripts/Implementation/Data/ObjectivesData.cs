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
        private List<Restriction> _victoryСonditions;
        private List<Restriction> _defeatСonditions;

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
            _victoryСonditions = objectivesDef.VictoryСonditions;
            _defeatСonditions = objectivesDef.DefeatСonditions;
        }

        public int GetTurnsCount()
        {
            return _startTurnsCount;
        }

        public List<ScoreData> GetStartScoreValues()
        {
            return _startScoreValues;
        }

        public List<Restriction> GetVictoryСonditions()
        {
            return _victoryСonditions;
        }

        public List<Restriction> GetDefeatСonditions()
        {
            return _defeatСonditions;
        }
    }
}
