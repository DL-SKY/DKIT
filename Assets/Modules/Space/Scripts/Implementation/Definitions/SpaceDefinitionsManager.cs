namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Space definition catalog. Ship, module, and skill records are added in the data step.
    /// </summary>
    public sealed class SpaceDefinitionsManager
    {
        public bool IsReady { get; private set; }

        public void Init()
        {
            IsReady = true;
        }
    }
}
