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
            // Доступные фишки и их веса
            _availableGems = new Dictionary<string, int>();
            FillRequiredGems(gameZoneGemsDef, ref _availableGems);
            FillAdditionalGems(gameZoneGemsDef, ref _availableGems);

            // Весовой калькулятор-рандомайзер
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

        private void AddGem(string gemDefId, int weight, ref Dictionary<string, int> availableGems)
        {
            if (!availableGems.ContainsKey(gemDefId))
                availableGems.Add(gemDefId, 0);

            availableGems[gemDefId] += weight;
        }

        private void FillRequiredGems(GameZoneGemsDef gameZoneGemsDef, ref Dictionary<string, int> availableGems)
        {
            foreach (var gemData in gameZoneGemsDef.Gems)
                AddGem(gemData.Key, gemData.Value, ref availableGems);
        }

        private void FillAdditionalGems(GameZoneGemsDef gameZoneGemsDef, ref Dictionary<string, int> availableGems)
        {
            if (gameZoneGemsDef.AdditionalGemsCount < 1)
                return;

            if (gameZoneGemsDef.AdditionalGems == null || gameZoneGemsDef.AdditionalGems.Count < 1)
                return;

            var weight = new List<KeyValuePair<KeyValuePair<string, int>, int>>();
            foreach (var gem in gameZoneGemsDef.AdditionalGems)
                weight.Add(new KeyValuePair<KeyValuePair<string, int>, int>(gem.Key, gem.Value));
            
            var additionalCalculator = new WeightsCalculator<KeyValuePair<string, int>>(new System.Random(), weight);
            var additionalGems = additionalCalculator.GetRandom(gameZoneGemsDef.AdditionalGemsCount, true);
            foreach (var additionalGem in additionalGems)
                AddGem(additionalGem.Key, additionalGem.Value, ref availableGems);
        }
    }
}
