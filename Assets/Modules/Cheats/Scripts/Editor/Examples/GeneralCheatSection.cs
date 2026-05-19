using Modules.Cheats.Scripts.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Modules.Cheats.Scripts.Editor.Examples
{
    public sealed class GeneralCheatSection : CheatSectionBase
    {
        private int _sampleValue;

        public override string Id => "General";

        public override int Order => 0;

        public override void DrawContent()
        {
            DrawSectionHeader("Base section template");
            DrawHelpInfo("This is a placeholder section. Add project-specific cheats in Implementation classes.");

            _sampleValue = EditorGUILayout.IntField("Sample value", _sampleValue);
            GUI.enabled = false;
            GUILayout.Button("Action placeholder", GUILayout.Height(24f));
            GUI.enabled = true;
        }
    }
}
