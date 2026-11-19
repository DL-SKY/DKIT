using Modules.Initializer.Scripts.Tasks;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Space language step. The catalog is added with Space definitions, not through Adventure.
    /// </summary>
    public sealed class SpaceLocalizationInitTask : TaskBase
    {
        public SpaceLocalizationInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            UnityEngine.Debug.Log("[Space] Localization catalog is not installed yet. Startup continues.");
            Complete();
        }
    }
}
