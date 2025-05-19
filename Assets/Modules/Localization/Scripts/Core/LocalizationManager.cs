using System;
using UnityEngine;

namespace Modules.Localization.Scripts.Core
{
    public class LocalizationManager
    {
        public Action<SystemLanguage> OnChangeLanguage;


        public void Init()
        {
            //UnityEngine.Application.systemLanguage

            //...
        }


        public bool TrySetLanguage(SystemLanguage newLanguage)
        {
            //...

            return false;
        }

        public string GetString(string key)
        {
            //...

            return string.Empty;
        }


        private void ApplyLanguage()
        { 
            //...
        }

        private void LoadLanguage(SystemLanguage language)
        { 
            //...
        }
    }
}
