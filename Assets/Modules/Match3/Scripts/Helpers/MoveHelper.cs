namespace Modules.Match3.Scripts.Helpers
{
    public static class MoveHelper
    {
        /// <summary>
        /// Функция плавного перехода (smoothstep) для более естественной анимации
        /// </summary>
        public static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
