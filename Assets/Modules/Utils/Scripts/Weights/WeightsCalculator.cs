using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Utils.Scripts.Weights
{
    public class WeightsCalculator<T>
    {
        private List<KeyValuePair<T, int>> _weights;
        private Random _random;

        public WeightsCalculator(Random random, List<KeyValuePair<T, int>> weights)
        {
            Reinitialize(random, weights);
        }

        public void Reinitialize(Random random, List<KeyValuePair<T, int>> weights)
        {
            _random = random;
            _weights = weights;
        }

        public T GetRandom(bool withDeleteItem = false)
        {
            var allWeights = _weights.Sum(x => x.Value);
            var result = _random.Next(0, allWeights);
            var min = 0;
            var max = 0;

            for (int i = 0; i < _weights.Count; i++)
            {
                var item = _weights[i];

                max = min + item.Value;
                if (min <= result && result < max)
                {
                    if (withDeleteItem)
                        _weights.RemoveAt(i);
                    return item.Key;
                }

                min += item.Value;
            }

            throw new Exception($"Wrong random! Weights.Count: {_weights.Count}");
        }

        public T[] GetRandom(int count, bool unique)
        {
            var result = new T[count];
            var originalWeights = unique ? new List<KeyValuePair<T, int>>(_weights) : _weights;


            for (int i = 0; i < count; i++)
            {
                result[i] = GetRandom(unique);
            }

            if (unique)
            {
                _weights.Clear();
                _weights = originalWeights;
            }

            return result;
        }

        public T GetRandom(List<T> excludedList)
        {
            // Создаем список доступных элементов, исключая элементы из excludedList
            var availableItems = new List<KeyValuePair<T, int>>();
            
            for (int i = 0; i < _weights.Count; i++)
            {
                var item = _weights[i];
                // Исключаем элементы из excludedList (если список не null и не пустой)
                if (excludedList == null || excludedList.Count == 0 || !excludedList.Contains(item.Key))
                {
                    availableItems.Add(item);
                }
            }

            if (availableItems.Count == 0)
                throw new Exception($"No available items! All items are excluded. Weights.Count: {_weights.Count}");

            // Вычисляем сумму весов доступных элементов
            var allWeights = availableItems.Sum(x => x.Value);
            if (allWeights == 0)
                throw new Exception($"Sum of available weights is zero! Weights.Count: {_weights.Count}");

            // Выбираем случайное число
            var result = _random.Next(0, allWeights);
            var min = 0;
            var max = 0;

            // Находим выбранный элемент
            for (int i = 0; i < availableItems.Count; i++)
            {
                var item = availableItems[i];
                max = min + item.Value;
                
                if (min <= result && result < max)
                {
                    return item.Key;
                }

                min += item.Value;
            }

            throw new Exception($"Wrong random! AvailableItems.Count: {availableItems.Count}");
        }

#if UNITY_EDITOR
        /// <summary>
        /// !!! ONLY IN UNITY_EDITOR !!!
        /// </summary>
        /// <param name="iterations"></param>
        public void StartTest(int iterations)
        {
            UnityEngine.Debug.LogError($" ================ WeightsCalculator.StartTest({iterations}) ================ ");

            var results = new List<T>(iterations);
            var counts = new Dictionary<T, int>();
            for (int i = 0; i < iterations; i++)
            {
                var result = GetRandom();
                results.Add(result);

                if (counts.ContainsKey(result))
                    counts[result]++;
                else
                    counts.Add(result, 1);

                UnityEngine.Debug.LogError($"{i + 1}: {result}");
            }            

            UnityEngine.Debug.LogError($" ================ Results ================ ");
            UnityEngine.Debug.LogError($" All Weights = {_weights.Sum(x => x.Value)}");
            UnityEngine.Debug.LogError($" Weights: {string.Join(", ", _weights)}");
            UnityEngine.Debug.LogError($" Counts: {string.Join(", ", counts)}");
            UnityEngine.Debug.LogError($" Items: {_weights.Count} //// HashSet.Count: {new HashSet<T>(results).Count}");
            UnityEngine.Debug.LogError($" ================ Test Completed ================ ");
        }
#endif
    }
}
