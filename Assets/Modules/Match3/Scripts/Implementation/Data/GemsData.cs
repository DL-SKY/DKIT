using Modules.Definitions.Scripts.Implementation.Defs.GameZoneGems;
using Modules.Match3.Scripts.Interfaces;
using Modules.Utils.Scripts.Weights;
using System.Collections.Generic;

namespace Modules.Match3.Scripts.Implementation.Data
{
    public class GemsData : IGemsData
    {
        private readonly Dictionary<string, int> _availableGems;
        private readonly WeightsCalculator<string> _calculator;

        public GemsData(GameZoneGemsDef gameZoneGemsDef)
        {
            _availableGems = new Dictionary<string, int>();
            foreach (var gemData in gameZoneGemsDef.Gems)
                _availableGems.Add(gemData.Key, gemData.Value);

            var weight = new List<KeyValuePair<string, int>>();
            foreach (var gem in _availableGems)
                weight.Add(new KeyValuePair<string, int>(gem.Key, gem.Value));
            _calculator = new WeightsCalculator<string>(new System.Random(), weight);
        }

        public Dictionary<string, int> GetAvailableGems()
        {
            return _availableGems;
        }

        public string GetRandomGem()
        {
            return _calculator.GetRandom();
        }

        public string GetRandomGem(List<string> excludedId)
        {
            return _calculator.GetRandom(excludedId);
        }
    }
}
