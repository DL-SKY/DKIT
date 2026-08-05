using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.TopPanel
{
    public class CreateCharacterParameterViewModel : AdventureParameterViewModelBase
    {
        private readonly CreateCharacterRequestData _requestData;

        public CreateCharacterParameterViewModel(CreateCharacterRequestData requestData)
        {
            _requestData = requestData;
        }

        protected override void Subscribe()
        {
            if (_requestData == null)
                return;

            _requestData.OnUpdate += OnUpdateHandler;
        }

        protected override void Unsubscribe()
        {
            if (_requestData == null)
                return;

            _requestData.OnUpdate -= OnUpdateHandler;
        }

        private void OnUpdateHandler()
        {
            if (IsDisposed)
                return;

            RefreshValue(notify: true);
        }

        protected override int ResolveParameterValue()
        {
            if (string.IsNullOrEmpty(ParameterName) || _requestData == null)
                return 0;

            var proxy = new CreatedCharacterParametersProxy(_requestData, ResolveRuleDef(), _definitionsManager);
            return proxy.GetTotalValue(ParameterName);
        }
    }
}
