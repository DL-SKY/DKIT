using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Modules.State.Scripts.Implementation.Adventure.StateDatas
{
    public class LocalizationStateData
    {
        public LocalizationStateData()
        {
            Language = SystemLanguage.Unknown;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public SystemLanguage Language;
    }
}
