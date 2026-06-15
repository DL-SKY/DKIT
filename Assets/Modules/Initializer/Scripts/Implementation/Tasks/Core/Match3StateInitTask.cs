using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Initializer.Scripts.Tasks;
using Modules.State.Scripts.Implementation.Match3;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class Match3StateInitTask : TaskBase
    {
        [Inject] private readonly Match3StateManager _stateManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public Match3StateInitTask(int weight) : base(weight)
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
