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
        public IReadOnlyDictionary<string, SpaceCommandDef> Commands => _commands;
        public IReadOnlyDictionary<string, SpaceTimedTargetDef> TimedTargets => _timedTargets;

        private Dictionary<string, SpaceHullDef> _hulls = new Dictionary<string, SpaceHullDef>();
        private Dictionary<string, SpaceModuleDef> _modules = new Dictionary<string, SpaceModuleDef>();
        private Dictionary<string, SpaceCommandDef> _commands = new Dictionary<string, SpaceCommandDef>();
        private Dictionary<string, SpaceTimedTargetDef> _timedTargets = new Dictionary<string, SpaceTimedTargetDef>();

        public void Init()
        {
            _hulls = SpaceDefinitionLoader.Load<SpaceHullDef>(SpaceDefinitionLoader.HULLS_PATH);
            _modules = SpaceDefinitionLoader.Load<SpaceModuleDef>(SpaceDefinitionLoader.MODULES_PATH);
            _commands = SpaceDefinitionLoader.Load<SpaceCommandDef>(SpaceDefinitionLoader.COMMANDS_PATH);
            _timedTargets = SpaceDefinitionLoader.Load<SpaceTimedTargetDef>(SpaceDefinitionLoader.TIMED_TARGETS_PATH);
            IsReady = HasRequiredRecords();
        }

        public bool TryGetHull(string hullId, out SpaceHullDef hull)
        {
            hull = null;
            return _hulls != null && !string.IsNullOrEmpty(hullId) && _hulls.TryGetValue(hullId, out hull);
        }

        public bool TryGetModule(string moduleId, out SpaceModuleDef module)
        {
            module = null;
            return _modules != null && !string.IsNullOrEmpty(moduleId) && _modules.TryGetValue(moduleId, out module);
        }

        public bool TryGetTimedTarget(string targetId, out SpaceTimedTargetDef target)
        {
            target = null;
            return _timedTargets != null && _timedTargets.TryGetValue(targetId, out target);
        }

        private bool HasRequiredRecords()
        {
            if (_hulls.Count < 1 || _modules.Count < 1 || _commands.Count < 1)
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
