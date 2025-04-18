using Modules.Windows.Scripts.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Examples
{
    public class TestView : ViewBase<TestViewModel>
    {
        public static string Path = "Examples/TestView";

        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveAllListeners();
        }

        public void OnClick()
        {
            _viewModel.OnClick();
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
