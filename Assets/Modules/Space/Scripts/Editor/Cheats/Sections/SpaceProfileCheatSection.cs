using System;
using System.Collections.Generic;
using System.IO;
using Modules.Cheats.Scripts.Editor.Core;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.Space.Scripts.Implementation.State;
using Modules.Space.Scripts.Implementation.State.Actions;
using Modules.State.Scripts.Actions.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Modules.Space.Scripts.Editor.Cheats.Sections
{
    public sealed class SpaceProfileCheatSection : CheatSectionBase
    {
        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        private GUIStyle _pathStyle;

        public override string Id => "Profile";

        public override int Order => -999;

        public override void DrawContent()
        {
            string directory = Path.Combine(Application.persistentDataPath, SpaceStateManager.SAVE_FOLDER);
            string filePath = Path.Combine(directory, SpaceStateManager.PROFILE_FILE_NAME);
            GUIStyle pathStyle = GetPathStyle();

            EditorGUILayout.LabelField("Profile file:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(filePath, pathStyle);
            EditorGUILayout.Space(6f);

            if (GUILayout.Button("Open profile folder", GUILayout.Height(24f)))
            {
                Directory.CreateDirectory(directory);
                EditorUtility.RevealInFinder(directory);
            }

            EditorGUILayout.Space(8f);

            SpaceProfile profile = TryGetLive(out SpaceStateManager state, out SpaceDefinitionsManager definitions);
            if (profile == null)
                profile = TryReadProfile(filePath);

            if (profile == null)
            {
                EditorGUILayout.HelpBox("Профиль Space ещё не создан.", MessageType.Info);
                return;
            }

            DrawSectionHeader(state != null ? "Space profile (play):" : "Space profile (file):");
            DrawShip(profile, state, definitions);
            EditorGUILayout.Space(6f);
            DrawQueues(profile);
        }

        private static SpaceProfile TryGetLive(out SpaceStateManager state, out SpaceDefinitionsManager definitions)
        {
            state = null;
            definitions = null;
            if (!Application.isPlaying || !ProjectContext.HasInstance)
                return null;

            DiContainer container = ProjectContext.Instance.Container;
            state = container.TryResolve<SpaceStateManager>();
            definitions = container.TryResolve<SpaceDefinitionsManager>();
            if (state == null || !state.IsReady || definitions == null || !definitions.IsReady)
            {
                state = null;
                definitions = null;
                return null;
            }

            return state.Profile;
        }

        private static SpaceProfile TryReadProfile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<SpaceProfile>(json, SETTINGS);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError("[Space] Failed to read profile for the cheat window. " + exception.Message);
                return null;
            }
        }

        private static void DrawShip(SpaceProfile profile, SpaceStateManager state, SpaceDefinitionsManager definitions)
        {
            string activeShip = string.IsNullOrEmpty(profile.ActiveShipId) ? "—" : profile.ActiveShipId;
            EditorGUILayout.LabelField("Active ship: " + activeShip);

            EditorGUILayout.LabelField("Ships:");
            EditorGUI.indentLevel++;
            List<SpacePlayerShipState> ships = profile.Ships;
            if (ships == null || ships.Count == 0)
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);
            else
            {
                for (int i = 0; i < ships.Count; i++)
                {
                    SpacePlayerShipState ship = ships[i];
                    if (ship == null)
                        continue;

                    string name = string.IsNullOrEmpty(ship.Name) ? "—" : ship.Name;
                    EditorGUILayout.LabelField((i + 1) + ". " + ship.Id + "  " + name + "  " + ship.HullId);
                    DrawSlots(profile, ship, state, definitions);
                }
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawSlots(SpaceProfile profile, SpacePlayerShipState ship, SpaceStateManager state, SpaceDefinitionsManager definitions)
        {
            List<SpacePlayerShipSlot> slots = ship.Slots;
            if (slots == null)
                return;

            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                SpacePlayerShipSlot slot = slots[slotIndex];
                if (slot == null)
                    continue;

                string moduleId = string.IsNullOrEmpty(slot.ModuleId) ? "—" : slot.ModuleId;
                EditorGUILayout.LabelField("    " + slot.SlotType + " " + slot.SizeType + "  " + moduleId);
                if (state == null || definitions == null)
                    continue;

                if (!string.IsNullOrEmpty(slot.ModuleId))
                {
                    if (GUILayout.Button("Remove", GUILayout.Height(20f)))
                        state.TryRemoveModule(ship.Id, slotIndex);
                    continue;
                }

                List<string> fits = ModulesThatFit(profile, definitions, ship.Id, slotIndex);
                if (fits.Count == 0)
                {
                    EditorGUILayout.LabelField("    no module fits", EditorStyles.miniLabel);
                    continue;
                }

                for (int moduleIndex = 0; moduleIndex < fits.Count; moduleIndex++)
                {
                    string candidate = fits[moduleIndex];
                    if (GUILayout.Button("Install " + candidate, GUILayout.Height(20f)))
                        state.TryInstallModule(ship.Id, slotIndex, candidate);
                }
            }
        }

        private static List<string> ModulesThatFit(SpaceProfile profile, SpaceDefinitionsManager definitions, string shipId, int slotIndex)
        {
            var fits = new List<string>();
            IReadOnlyDictionary<string, SpaceModuleDef> modules = definitions.Modules;
            if (modules == null)
                return fits;

            foreach (KeyValuePair<string, SpaceModuleDef> pair in modules)
            {
                if (string.IsNullOrEmpty(pair.Key))
                    continue;

                var action = new InstallSpaceModuleStateAction(definitions, shipId, slotIndex, pair.Key);
                StateActionValidationResult result = action.Validate(profile);
                if (result != null && result.IsValid)
                    fits.Add(pair.Key);
            }

            fits.Sort(StringComparer.Ordinal);
            return fits;
        }

        private static void DrawQueues(SpaceProfile profile)
        {
            List<SpaceQueueState> queues = profile.Queues;
            if (queues == null || queues.Count == 0)
            {
                EditorGUILayout.LabelField("Queues: —", EditorStyles.miniLabel);
                return;
            }

            long nowUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            for (int i = 0; i < queues.Count; i++)
            {
                SpaceQueueState queue = queues[i];
                if (queue == null)
                    continue;

                int rowCount = queue.Rows != null ? queue.Rows.Count : 0;
                EditorGUILayout.LabelField(queue.Id + "  " + rowCount + "/" + queue.MaxCount);
                EditorGUI.indentLevel++;
                DrawRows(queue.Rows, nowUnixMs);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawRows(List<SpaceQueueRow> rows, long nowUnixMs)
        {
            if (rows == null || rows.Count == 0)
            {
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);
                return;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                SpaceQueueRow row = rows[i];
                if (row == null)
                    continue;

                string state = row.IsStarted ? "running " + row.GetRemainingMs(nowUnixMs) + " ms" : "waiting";
                EditorGUILayout.LabelField((i + 1) + ". " + row.Kind + " " + row.TargetId + "  " + row.BaseTimeMs + " ms  " + state);
            }
        }

        private GUIStyle GetPathStyle()
        {
            if (_pathStyle != null)
                return _pathStyle;

            _pathStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                wordWrap = true
            };
            return _pathStyle;
        }
    }
}
