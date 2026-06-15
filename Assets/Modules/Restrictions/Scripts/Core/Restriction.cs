using System.Collections.Generic;

namespace Modules.Restrictions.Scripts.Core
{
    /// <summary>
    /// Данные рестрикшена (ограничения).
    /// Содержит тип ограничения, значения для сравнения и тип операции сравнения.
    /// </summary>
    public class Restriction
    {
        /// <summary>
        /// Тип рестрикшена (идентификатор для определения нужного чекера)
        /// </summary>
        public RestrictionType Type;

        /// <summary>
        /// Строковые значения для сравнения
        /// </summary>
        public List<string> StringValues;

        /// <summary>
        /// Целочисленные значения для сравнения
        /// </summary>
        public List<int> IntValues;

        /// <summary>
        /// Длинные целочисленные значения для сравнения (например, для времени в миллисекундах)
        /// </summary>
        public List<long> LongValues;

        /// <summary>
        /// Тип операции сравнения
        /// </summary>
        public CompareType CompareOptions;
    }
}

