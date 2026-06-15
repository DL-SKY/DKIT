using System.Collections.Generic;

namespace Modules.Match3.Scripts.Interfaces
{
    /// <summary>
    /// Интерфейс для получения данных о доступных гемах в игре Match3.
    /// </summary>
    public interface IGemsData
    {
        /// <summary>
        /// Получает словарь доступных гемов с их весами для рандомизации.
        /// </summary>
        /// <returns>Key - gem ID, Value - weight for randomize</returns>
        Dictionary<string, int> GetAvailableGems();

        /// <summary>
        /// Получает случайный ID гема на основе весов из доступных гемов.
        /// </summary>
        /// <returns>ID случайно выбранного гема</returns>
        string GetRandomGem();

        /// <summary>
        /// Получает случайный ID гема на основе весов из доступных гемов, исключая указанные ID.
        /// </summary>
        /// <param name="excludedId">Список ID гемов, которые должны быть исключены из выборки</param>
        /// <returns>ID случайно выбранного гема</returns>
        string GetRandomGem(List<string> excludedId);
    }
}
