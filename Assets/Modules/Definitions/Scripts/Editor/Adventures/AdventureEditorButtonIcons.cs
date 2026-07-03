using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    /// <summary>
    /// Resolves TEA create-button icons: custom PNGs from <see cref="ICONS_FOLDER"/> first,
    /// then Unity built-in editor icons via <see cref="EditorGUIUtility.IconContent"/>.
    /// Assets under an Editor folder are excluded from player builds.
    /// </summary>
    public static class AdventureEditorButtonIcons
    {
        public const string ICONS_FOLDER = "Assets/Modules/Definitions/Scripts/Editor/Adventures/ButtonIcons";

        public static Texture Resolve(string iconName, string iconAssetName = null)
        {
            Texture customIcon = LoadCustomIcon(iconAssetName);
            if (customIcon != null)
                return customIcon;

            if (string.IsNullOrWhiteSpace(iconName))
                return null;

            return EditorGUIUtility.IconContent(iconName)?.image;
        }

        private static Texture LoadCustomIcon(string iconAssetName)
        {
            if (string.IsNullOrWhiteSpace(iconAssetName))
                return null;

            string fileName = iconAssetName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                ? iconAssetName
                : $"{iconAssetName}.png";

            string assetPath = $"{ICONS_FOLDER}/{fileName}";
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
    }
}
