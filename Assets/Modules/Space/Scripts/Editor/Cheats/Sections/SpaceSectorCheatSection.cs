using System.Collections.Generic;
using Modules.Cheats.Scripts.Editor.Core;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Modules.Space.Scripts.Editor.Cheats.Sections
{
    public sealed class SpaceSectorCheatSection : CheatSectionBase
    {
        public override string Id => "Sector";

        public override int Order => -997;

        public override void DrawContent()
        {
            if (!Application.isPlaying || !ProjectContext.HasInstance)
            {
                EditorGUILayout.HelpBox("Сектор ещё не загружен.", MessageType.Info);
                return;
            }

            SpaceDefinitionsManager definitions = ProjectContext.Instance.Container.TryResolve<SpaceDefinitionsManager>();
            SpaceSessionHost host = ProjectContext.Instance.Container.TryResolve<SpaceSessionHost>();
            if (definitions == null || !definitions.IsReady || !TryGetCurrentSector(definitions, out SpaceSectorDef sector))
            {
                EditorGUILayout.HelpBox("Открытая сцена не совпадает с дефом сектора.", MessageType.Info);
                return;
            }

            DrawSectionHeader(sector.Id + "  " + sector.SceneName);
            if (sector.Placements == null || sector.Placements.Count == 0)
            {
                EditorGUILayout.LabelField("—", EditorStyles.miniLabel);
                return;
            }

            for (int i = 0; i < sector.Placements.Count; i++)
            {
                SpaceSectorPlacement placement = sector.Placements[i];
                if (placement == null || string.IsNullOrEmpty(placement.ObjectId))
                    continue;

                if (!definitions.TryGetObject(placement.ObjectId, out SpaceObjectDef body) || body == null)
                    continue;

                string label = (i + 1) + ". " + body.Id + "  " + body.ObjectType
                    + "  (" + placement.X + ", " + placement.Y + ", " + placement.Z + ")";
                if (!GUILayout.Button(label, GUILayout.Height(22f)))
                    continue;

                if (host == null)
                {
                    UnityEngine.Debug.LogError("[Space] Move target was not set. Session host is missing.");
                    continue;
                }

                host.TrySetMoveTarget(SpaceSectorBodies.PlacementKey(i));
            }
        }

        private static bool TryGetCurrentSector(SpaceDefinitionsManager definitions, out SpaceSectorDef sector)
        {
            sector = null;
            string sceneName = SceneManager.GetActiveScene().name;
            if (definitions.Sectors == null || string.IsNullOrEmpty(sceneName))
                return false;

            foreach (KeyValuePair<string, SpaceSectorDef> pair in definitions.Sectors)
            {
                if (pair.Value == null || pair.Value.SceneName != sceneName)
                    continue;

                sector = pair.Value;
                return true;
            }

            return false;
        }
    }
}
