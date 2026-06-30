using System;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures.CreateOptions
{
    public sealed class CreateOptionDescriptor<T>
    {
        public string Id;
        public string ButtonText;
        public string Tooltip;
        public string IconName;
        public Func<T> Create;

        public GUIContent ToGuiContent()
        {
            Texture icon = string.IsNullOrWhiteSpace(IconName)
                ? null
                : EditorGUIUtility.IconContent(IconName)?.image;

            return new GUIContent(ButtonText, icon, Tooltip);
        }
    }
}
