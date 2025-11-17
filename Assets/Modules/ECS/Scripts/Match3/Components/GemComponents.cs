using Modules.Match3.Scripts.Implementation.Visual;

namespace Modules.ECS.Scripts.Match3.Components
{
    // Компонент типа фишки
    public struct GemType
    {
        public string Type;
    }

    // Компонент визуала фишки
    public struct GemView
    { 
        public GemVisual GemVisual;
    }
}
