using Modules.Definitions.Scripts.Implementation.Defs;
using Modules.Save.Scripts.Interfaces;
using Modules.State.Scripts.Core;
using Modules.Utils.Scripts.Extensions;
using System;
using Zenject;


namespace Modules.State.Scripts.Implementation.Match3
{
    public class Match3StateManager : StateManager<StateData>
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public void SaveState()
        {
            var settings = _definitionsManager.GlobalSettings;
            SaveProfileState(settings.SaveName, settings.EnabledEncryption, settings.EncryptionKey);

            
        }

        protected override void OnApplicationQuitting()
        {
            SaveState();
        }

        protected override StateData CreateNewState(string profileId)
        {
            return new StateData
            {
                //ProfileId = profileId,
                //PlayerLevel = 1,
                //Coins = 0,
                //Lives = 5,
                //CurrentRoundDefId = "RoundExample",
                //LastSaveUtcTicks = DateTime.UtcNow.Ticks
                // example
                //var r = DateTime.UtcNow.ToUnixMs();
            };
        }
    }
}
