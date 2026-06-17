using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Initializer.Scripts.Tasks;
using Modules.State.Scripts.Implementation.Adventure;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class AdventureStateInitTask : TaskBase
    {
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public AdventureStateInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            var settings = _definitionsManager.GlobalSettings;

            _stateManager.Init(settings.SaveFolder, settings.FileExtension, settings.EncryptionKey);
            _stateManager.LoadProfileState(settings.SaveName, settings.EnabledEncryption, settings.EncryptionKey);

            Complete();
        }
    }
}
