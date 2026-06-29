using Modules.Definitions.Scripts.Defs;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Definitions.Scripts.Implementation.Defs.Single
{
    public class LocalizationSettingsDef : AbstractDefinition
    {
        public SystemLanguage DefaultLanguage;
        public Dictionary<SystemLanguage, string> LanguageFolders;
    }
}
