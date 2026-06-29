using Modules.Initializer.Scripts.Tasks;
using Modules.Localization.Scripts.Implementation;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Interfaces;
using Modules.Utils.Scripts.Components;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Modules.Initializer.Scripts.Implementation.Tasks.Core
{
    public class LocalizationInitTask : TaskBase
    {
        [Inject] private readonly CoroutineHolder _coroutineHolder;
        [Inject] private readonly LocalizationManager _localizationManager;

        [InjectOptional] private readonly AdventureStateManager _adventureStateManager;
        [InjectOptional] private readonly LazyInject<Modules.State.Scripts.Implementation.Adventure.Logic.AdventureStateLogic> _adventureStateLogic;

        public LocalizationInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            _coroutineHolder.StartCoroutine(InitAsync());
        }

        private IEnumerator InitAsync()
        {
            var stateLanguage = GetLanguageFromStateOrUnknown();
            var initLanguage = stateLanguage == SystemLanguage.Unknown
                ? _localizationManager.ResolveSystemLanguageOrDefault()
                : stateLanguage;

            if (!_localizationManager.TrySetLanguage(initLanguage))
            {
                var fallbackLanguage = _localizationManager.GetDefaultLanguage();
                if (!_localizationManager.TrySetLanguage(fallbackLanguage))
                {
                    UnityEngine.Debug.LogWarning("[LocalizationInitTask] Failed to initialize localization with init and default languages.");
                    Complete();
                    yield break;
                }

                initLanguage = fallbackLanguage;
            }

            SaveLanguageToState(initLanguage);
            yield return null;

            Complete();
        }

        private SystemLanguage GetLanguageFromStateOrUnknown()
        {
            var owner = _adventureStateManager?.State as ILocalizationStateDataOwner;
            if (owner == null)
                return SystemLanguage.Unknown;

            var localizationState = owner.GetLocalizationStateData();
            if (localizationState == null)
                return SystemLanguage.Unknown;

            return localizationState.Language;
        }

        private void SaveLanguageToState(SystemLanguage language)
        {
            if (!(_adventureStateManager?.State is ILocalizationStateDataOwner))
                return;

            if (_adventureStateLogic == null)
                return;

            var actionResult = _adventureStateLogic.Value.ProcessAction(
                new SetLocalizationLanguageStateAction<Modules.State.Scripts.Implementation.Adventure.StateData>(language),
                true);
            if (!actionResult.IsValid)
            {
                UnityEngine.Debug.LogWarning($"[LocalizationInitTask] Failed to save language to state: {actionResult.ErrorMessage}");
            }
        }
    }
}
