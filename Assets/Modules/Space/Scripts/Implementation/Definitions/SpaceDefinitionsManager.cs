using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Queues;

namespace Modules.Space.Scripts.Implementation.Definitions
{
    /// <summary>
    /// Space definition catalog. Records live in Space Resources, not in the Adventure catalog.
    /// </summary>
    public sealed class SpaceDefinitionsManager
    {
        public bool IsReady { get; private set; }
        public IReadOnlyDictionary<string, SpaceHullDef> Hulls => _hulls;
        public IReadOnlyDictionary<string, SpaceModuleDef> Modules => _modules;
        public IReadOnlyDictionary<string, SpaceControlOrganDef> ControlOrgans => _controlOrgans;
        public IReadOnlyDictionary<string, SpaceTimedTargetDef> TimedTargets => _timedTargets;

        private Dictionary<string, SpaceHullDef> _hulls = new Dictionary<string, SpaceHullDef>();
        private Dictionary<string, SpaceModuleDef> _modules = new Dictionary<string, SpaceModuleDef>();
        private Dictionary<string, SpaceControlOrganDef> _controlOrgans = new Dictionary<string, SpaceControlOrganDef>();
        private Dictionary<string, SpaceTimedTargetDef> _timedTargets = new Dictionary<string, SpaceTimedTargetDef>();

        public void Init()
        {
            _hulls = SpaceDefinitionLoader.Load<SpaceHullDef>(SpaceDefinitionLoader.HULLS_PATH);
            _modules = SpaceDefinitionLoader.Load<SpaceModuleDef>(SpaceDefinitionLoader.MODULES_PATH);
            _controlOrgans = SpaceDefinitionLoader.Load<SpaceControlOrganDef>(SpaceDefinitionLoader.CONTROL_ORGANS_PATH);
            _timedTargets = SpaceDefinitionLoader.Load<SpaceTimedTargetDef>(SpaceDefinitionLoader.TIMED_TARGETS_PATH);
            IsReady = HasRequiredRecords();
        }

        public bool TryGetTimedTarget(string targetId, out SpaceTimedTargetDef target)
        {
            target = null;
            return _timedTargets != null && _timedTargets.TryGetValue(targetId, out target);
        }

        private bool HasRequiredRecords()
        {
            if (_hulls.Count < 1 || _modules.Count < 1 || _controlOrgans.Count < 1)
                return false;

            var skillDurations = new HashSet<long>();
            var kinds = new HashSet<SpaceQueueKind>();
            foreach (var pair in _timedTargets)
            {
                var target = pair.Value;
                if (target == null || target.BaseTimeMs < SpaceQueueTime.MINIMUM_MS)
                    return false;

                kinds.Add(target.Kind);
                if (target.Kind == SpaceQueueKind.Skill)
                    skillDurations.Add(target.BaseTimeMs);
            }

            return skillDurations.Count >= 3
                && kinds.Contains(SpaceQueueKind.Skill)
                && kinds.Contains(SpaceQueueKind.BlueprintImprovement)
                && kinds.Contains(SpaceQueueKind.Disassembly)
                && kinds.Contains(SpaceQueueKind.BlueprintAttempt)
                && kinds.Contains(SpaceQueueKind.Production);
        }
    }
}
