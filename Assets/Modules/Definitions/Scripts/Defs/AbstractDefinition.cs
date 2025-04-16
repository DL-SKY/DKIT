using Newtonsoft.Json;

namespace Modules.Definitions.Scripts.Defs
{
    public abstract class AbstractDefinition
    {
        [JsonIgnore]
        public string Id;
    }
}
