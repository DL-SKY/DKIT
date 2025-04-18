using System;
using UnityEngine;

namespace Assets.Modules.Utils.Scripts.Components
{
    public sealed class Updater : MonoBehaviour
    {
        public event Action<float> OnUpdate;
        public event Action<float> OnLateUpdate;


        private void Update()
        {
            OnUpdate?.Invoke(Time.deltaTime);
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke(Time.deltaTime);
        }
    }
}
