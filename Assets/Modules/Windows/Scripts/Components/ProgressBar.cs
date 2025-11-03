using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Components
{
    [RequireComponent(typeof(Image))]
    public class ProgressBar : MonoBehaviour
    {
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void SetFillAmount(float value)
        {
            _image.fillAmount = value;
        }
    }
}
