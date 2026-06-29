using Modules.Localization.Scripts.Implementation;
using Zenject;

namespace Modules.Localization.Scripts.Components
{
    public class LocalizationText : LocalizationTextProxy
    {
        [Inject] private readonly LocalizationManager _localizationManager;

        protected override void Init()
        {
            _manager = _localizationManager;
            _isInit = _manager != null;
        }
    }
}
