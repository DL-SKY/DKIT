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
        public IReadOnlyDictionary<string, SpaceSectorDef> Sectors => _sectors;
        public IReadOnlyDictionary<string, SpaceObjectDef> Objects => _objects;
        public SpaceCameraSettingsDef CameraSettings { get; private set; }
        public SpaceMarkerIconsDef MarkerIcons { get; private set; }

        private Dictionary<string, SpaceHullDef> _hulls = new Dictionary<string, SpaceHullDef>();
        private Dictionary<string, SpaceModuleDef> _modules = new Dictionary<string, SpaceModuleDef>();
        private Dictionary<string, SpaceCommandDef> _commands = new Dictionary<string, SpaceCommandDef>();
        private Dictionary<string, SpaceTimedTargetDef> _timedTargets = new Dictionary<string, SpaceTimedTargetDef>();
        private Dictionary<string, SpaceSectorDef> _sectors = new Dictionary<string, SpaceSectorDef>();
        private Dictionary<string, SpaceObjectDef> _objects = new Dictionary<string, SpaceObjectDef>();

        public void Init()
        {
            _hulls = SpaceDefinitionLoader.Load<SpaceHullDef>(SpaceDefinitionLoader.HULLS_PATH);
            _modules = SpaceDefinitionLoader.Load<SpaceModuleDef>(SpaceDefinitionLoader.MODULES_PATH);
            _commands = SpaceDefinitionLoader.Load<SpaceCommandDef>(SpaceDefinitionLoader.COMMANDS_PATH);
            _timedTargets = SpaceDefinitionLoader.Load<SpaceTimedTargetDef>(SpaceDefinitionLoader.TIMED_TARGETS_PATH);
            _sectors = SpaceDefinitionLoader.Load<SpaceSectorDef>(SpaceDefinitionLoader.SECTORS_PATH);
            _objects = SpaceDefinitionLoader.Load<SpaceObjectDef>(SpaceDefinitionLoader.OBJECTS_PATH);
            CameraSettings = SpaceDefinitionLoader.LoadSingle<SpaceCameraSettingsDef>(SpaceDefinitionLoader.CAMERA_SETTINGS_PATH);
            MarkerIcons = SpaceDefinitionLoader.LoadSingle<SpaceMarkerIconsDef>(SpaceDefinitionLoader.MARKER_ICONS_PATH);
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

        public bool TryGetSector(string sectorId, out SpaceSectorDef sector)
        {
            sector = null;
            return _sectors != null && !string.IsNullOrEmpty(sectorId) && _sectors.TryGetValue(sectorId, out sector);
        }

        public bool TryGetObject(string objectId, out SpaceObjectDef body)
        {
            body = null;
            return _objects != null && !string.IsNullOrEmpty(objectId) && _objects.TryGetValue(objectId, out body);
        }

        public bool TryGetCommand(string commandId, out SpaceCommandDef command)
        {
            command = null;
            return _commands != null && !string.IsNullOrEmpty(commandId) && _commands.TryGetValue(commandId, out command);
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

            if (CameraSettings == null || !CameraSettings.IsUsable() || MarkerIcons == null || !MarkerIcons.IsUsable())
                return false;

            if (!HullsFit() || !ModulesFit() || !ObjectsFit() || !SectorsFit())
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

        private bool HullsFit()
        {
            foreach (var pair in _hulls)
            {
                var hull = pair.Value;
                if (hull == null
                    || !hull.HasFlightRates()
                    || string.IsNullOrWhiteSpace(hull.IconResourcesPath)
                    || hull.CameraDistance < CameraSettings.MinDistance
                    || hull.CameraDistance > CameraSettings.MaxDistance)
                    return false;
            }

            return true;
        }

        private bool ModulesFit()
        {
            if (_modules == null || _modules.Count < 1)
                return false;

            var hasEngine = false;
            foreach (var pair in _modules)
            {
                var module = pair.Value;
                if (module == null || !module.IsUsable())
                    return false;

                if (module.SlotType == SpaceSlotType.Engine)
                    hasEngine = true;
            }

            return hasEngine;
        }

        private bool ObjectsFit()
        {
            if (_objects == null || _objects.Count < 1)
                return false;

            foreach (var pair in _objects)
            {
                if (pair.Value == null || !pair.Value.IsUsable())
                    return false;
            }

            return true;
        }

        private bool SectorsFit()
        {
            if (_sectors == null || _sectors.Count < 1)
                return false;

            foreach (var pair in _sectors)
            {
                if (pair.Value == null || !pair.Value.IsUsable(_objects))
                    return false;
            }

            return true;
        }
    }
}
