using Modules.Localization.Scripts.Implementation;
using Zenject;

namespace Modules.Localization.Scripts.Components
{
    public class LocalizationText : LocalizationTextProxy
    {
        [Inject] private LocalizationManager _localizationManager;

        protected override void Init()
        {
            if (_localizationManager == null)
                _localizationManager = ProjectContext.Instance.Container.TryResolve<LocalizationManager>();

            _manager = _localizationManager;
            _isInit = _manager != null;

            Unsubscribe();
            Subscribe();
        }
    }
}
