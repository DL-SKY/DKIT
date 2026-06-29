using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Interfaces;
using Modules.State.Scripts.Interfaces;
using UnityEngine;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class SetLocalizationLanguageStateAction<TStateData> : StateActionBase<TStateData>
        where TStateData : class, IStateData, ILocalizationStateDataOwner, new()
    {
        public override StateChangeSource Source => StateChangeSource.SetLocalizationLanguage;

        private readonly SystemLanguage _language;

        public SetLocalizationLanguageStateAction(SystemLanguage language)
        {
            _language = language;
        }

        public override StateActionValidationResult Validate(TStateData state)
        {
            var localizationState = state.GetLocalizationStateData();
            if (localizationState == null)
                return StateActionValidationResult.Fail("Localization state is null.", 40);

            if (_language == SystemLanguage.Unknown)
                return StateActionValidationResult.Fail("Localization language must not be Unknown.", 41);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(TStateData state)
        {
            var localizationState = state.GetLocalizationStateData();
            localizationState.Language = _language;
        }
    }
}
