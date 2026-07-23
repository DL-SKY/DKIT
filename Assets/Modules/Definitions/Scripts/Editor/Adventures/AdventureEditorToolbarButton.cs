using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    /// <summary>
    /// Main toolbar shortcut to open TEA. Does not change the Tools menu item.
    /// </summary>
    public static class AdventureEditorToolbarButton
    {
        private const string TOOLBAR_ELEMENT_PATH = "Definitions/Adventures/Adventure Editor";
        private const string ICON_ASSET_NAME = "WindowBoard";
        private const string TOOLTIP = "Adventure Editor (TEA)";

        // Dock to the middle strip (Play/Pause/Step), after the built-in controls.
        [MainToolbarElement(
            TOOLBAR_ELEMENT_PATH,
            defaultDockPosition = MainToolbarDockPosition.Middle,
            defaultDockIndex = 100)]
        public static MainToolbarElement Create()
        {
            Texture2D icon = AdventureEditorButtonIcons.Resolve(null, ICON_ASSET_NAME) as Texture2D;
            MainToolbarContent content = icon != null
                ? new MainToolbarContent("TEA", icon, TOOLTIP)
                : new MainToolbarContent("TEA", TOOLTIP);

            return new MainToolbarButton(content, OnClicked);
        }

        private static void OnClicked()
        {
            AdventureEditorWindow.Open();
        }
    }
}
