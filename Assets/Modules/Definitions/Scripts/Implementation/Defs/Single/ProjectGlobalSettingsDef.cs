using Modules.Definitions.Scripts.Defs;

namespace Modules.Definitions.Scripts.Implementation.Defs.Single
{
    public class ProjectGlobalSettingsDef : AbstractDefinition
    {
        public string SaveName;
        public string SaveFolder;
        public string FileExtension;
        public bool EnabledEncryption;
        public string EncryptionKey;
    }
}
