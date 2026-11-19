using System;
using Modules.Initializer.Scripts.Tasks;
using Modules.Localization.Scripts.Implementation;
using Zenject;

namespace Modules.Space.Scripts.Implementation.Initialization
{
    /// <summary>
    /// Sets the language for Space startup. Does not read or write the Adventure profile.
    /// </summary>
    public sealed class SpaceLocalizationInitTask : TaskBase
    {
        [Inject] private readonly LocalizationManager _localizationManager;

        public SpaceLocalizationInitTask(int weight) : base(weight)
        {
        }

        public override void Run()
        {
            if (_localizationManager == null)
            {
                Fail(SpaceInitErrorCodes.LOCALIZATION);
                return;
            }

            try
            {
                var language = _localizationManager.ResolveSystemLanguageOrDefault();
                if (!_localizationManager.TrySetLanguage(language))
                {
                    language = _localizationManager.GetDefaultLanguage();
                    if (!_localizationManager.TrySetLanguage(language))
                    {
                        UnityEngine.Debug.LogWarning("[Space] Localization settings are not available yet. Startup continues.");
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                UnityEngine.Debug.LogWarning($"[Space] Localization settings are not loaded yet: {exception.Message}");
            }

            Complete();
        }
    }
}
