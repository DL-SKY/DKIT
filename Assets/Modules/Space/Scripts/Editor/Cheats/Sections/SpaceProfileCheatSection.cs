using System;
using System.Collections.Generic;
using System.IO;
using Modules.Cheats.Scripts.Editor.Core;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.Space.Scripts.Implementation.State;
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

            SpaceProfile profile = TryGetLiveProfile(out bool isLive);
            if (profile == null)
                profile = TryReadProfile(filePath);

            if (profile == null)
            {
                EditorGUILayout.HelpBox("Профиль Space ещё не создан.", MessageType.Info);
                return;
            }

            DrawSectionHeader(isLive ? "Space profile (play):" : "Space profile (file):");
            DrawShip(profile);
            EditorGUILayout.Space(6f);
            DrawQueues(profile);
        }

        private static SpaceProfile TryGetLiveProfile(out bool isLive)
        {
            isLive = false;
            if (!Application.isPlaying || !ProjectContext.HasInstance)
                return null;

            SpaceStateManager state = ProjectContext.Instance.Container.TryResolve<SpaceStateManager>();
            if (state == null || !state.IsReady)
                return null;

            isLive = true;
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

        private static void DrawShip(SpaceProfile profile)
        {
            string activeHull = string.IsNullOrEmpty(profile.ActiveHullId) ? "—" : profile.ActiveHullId;
            EditorGUILayout.LabelField("Active hull: " + activeHull);

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

                    EditorGUILayout.LabelField((i + 1) + ". " + ship.HullId);
                    List<SpacePlayerShipSlot> slots = ship.Slots;
                    if (slots == null)
                        continue;

                    for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
                    {
                        SpacePlayerShipSlot slot = slots[slotIndex];
                        if (slot == null)
                            continue;

                        string moduleId = string.IsNullOrEmpty(slot.ModuleId) ? "—" : slot.ModuleId;
                        EditorGUILayout.LabelField("    " + slot.SlotType + " " + slot.SizeType + "  " + moduleId);
                    }
                }
            }

            EditorGUI.indentLevel--;
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
