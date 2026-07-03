using System;
using Modules.Definitions.Scripts.Editor.Adventures;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures.CreateOptions
{
    public sealed class CreateOptionDescriptor<T>
    {
        public string Id;
        public string ButtonText;
        public string Tooltip;
        public string IconName;
        public string IconAssetName;
        public Func<T> Create;

        public Texture ResolveIcon()
        {
            return AdventureEditorButtonIcons.Resolve(IconName, IconAssetName);
        }

        public GUIContent ToGuiContent()
        {
            return new GUIContent(ButtonText, ResolveIcon(), Tooltip);
        }
    }
}
