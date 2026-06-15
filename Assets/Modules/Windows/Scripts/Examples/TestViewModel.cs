using Modules.Dices.Scripts;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Settings;

namespace Modules.Windows.Scripts.Examples
{
    public class TestViewModel : ViewModelBase
    {
        public void OnClick()
        {
            Example.Manager.CloseView(_viewHandle);
        }

        public void OnRoll()
        {
            var dice = new Dice(DiceType.D20);
            var result = dice.Roll();

            UnityEngine.Debug.LogError($"result: {result.Result} ({result.Type})");
        }

        protected override Options CreateOptions()
        {
            //return new Options();
            return new Options(canCloseOnEsc: true, hideInHistory: false, sortingLayer: SortingOrderLayer.COMMON);
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
