using Modules.Windows.Scripts.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Examples
{
    public class TestView : ViewBase<TestViewModel>
    {
        public static string Path = "Examples/TestView";

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _rollButton;

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnClick);
            _rollButton.onClick.AddListener(OnRoll);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveAllListeners();
            _rollButton.onClick.RemoveAllListeners();
        }

        public void OnClick()
        {
            _viewModel.OnClick();
        }

        public void OnRoll()
        {
            _viewModel.OnRoll();
        }

        protected override void Subscribe()
        {
            //throw new System.NotImplementedException();
        }

        protected override void Unsubscribe()
        {
            //throw new System.NotImplementedException();
        }
    }
}
