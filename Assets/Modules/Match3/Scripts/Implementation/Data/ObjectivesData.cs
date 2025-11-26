using Modules.Definitions.Scripts.Implementation.Defs.Objectives;
using Modules.ECS.Scripts.Match3.Components;
using Modules.Match3.Scripts.Interfaces;
using System.Collections.Generic;

namespace Modules.Match3.Scripts.Implementation.Data
{
    public class ObjectivesData : IObjectivesData
    {
        private int _startTurnsCount;
        private List<ScoreData> _startScoreValues;

        public ObjectivesData(ObjectivesDef objectivesDef)
        {
            //TODO: ...
            // Подсчитать реальное значение стартовой величины Ходов
            _startTurnsCount = 0;

            // Стартовое значение очков
            _startScoreValues = new List<ScoreData>();
            foreach (var score in objectivesDef.StartScores)
                _startScoreValues.Add(score);

            //TODO: ...
            // Данные об условиях окончания игры (вероятно через рестрикшены?)
        }

        public int GetTurnsCount()
        {
            return _startTurnsCount;
        }

        public List<ScoreData> GetStartScoreValues()
        {
            return _startScoreValues;
        }
    }
}
