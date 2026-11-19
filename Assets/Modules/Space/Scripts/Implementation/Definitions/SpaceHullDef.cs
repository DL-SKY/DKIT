using System.Collections.Generic;
using Modules.Definitions.Scripts.Defs;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    public sealed class SpaceHullDef : AbstractDefinition
    {
        public List<string> ModuleSlotIds;
        public List<string> CommandIds;
        public string VisualResourcesPath;
        public float CameraDistance;
    }
}
