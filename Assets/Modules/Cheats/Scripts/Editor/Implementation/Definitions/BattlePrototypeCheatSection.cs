using Modules.Cheats.Scripts.Editor.Core;
using Modules.RPG.Scripts.Adventure.CursorBattlePrototype;
using UnityEditor;
using UnityEngine;

namespace Modules.Cheats.Scripts.Editor.Implementation.Definitions
{
    public sealed class BattlePrototypeCheatSection : CheatSectionBase
    {
        public override string Id => "Battle Prototype";

        public override int Order => -996;

        public override void DrawContent()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Open works only in Play Mode.", MessageType.Info);
                GUI.enabled = false;
                GUILayout.Button("Open Battle Prototype Window", GUILayout.Height(24f));
                GUI.enabled = true;
                return;
            }

            if (GUILayout.Button("Open Battle Prototype Window", GUILayout.Height(24f)))
            {
                CursorBattlePrototypeOpener.OpenFromCheats();
            }
        }
    }
}
