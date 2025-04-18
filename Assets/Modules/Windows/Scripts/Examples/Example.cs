using Modules.Windows.Scripts.Managers;
using System.Collections;
using UnityEngine;

namespace Modules.Windows.Scripts.Examples
{
    public class Example : MonoBehaviour
    {
        public static WindowsManager Manager;

        [SerializeField] private WindowsManager _windowsManager;

        private void Awake()
        {
            Manager = _windowsManager;
        }

        private void Start()
        {
            StartCoroutine(Show());
        }

        private IEnumerator Show()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(2.5f);

                var model = new TestViewModel();
                _windowsManager.OpenView<TestView, TestViewModel>(TestView.Path, model);
            }
        }
    }
}
