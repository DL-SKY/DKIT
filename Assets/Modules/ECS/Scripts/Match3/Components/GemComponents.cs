using Modules.Match3.Scripts.Implementation.Visual;

namespace Modules.ECS.Scripts.Match3.Components
{
    /// <summary>
    /// Компонент типа фишки
    /// </summary>
    public struct GemType
    {
        /// <summary>
        /// DefId фишки
        /// </summary>
        public string Type;
    }

    /// <summary>
    /// Компонент визуала фишки
    /// </summary>
    public struct GemView
    { 
        public GemVisual GemVisual;
    }
}
