#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

namespace Modules.Utils.Scripts.Editor.UnityUi
{
    /// <summary>
    /// Unity 6000.4 ugui bug (SelectableEditor.OnInspectorGUI): when Target Graphic is
    /// None and the Selectable has no Graphic on the same GameObject, assigning a new
    /// Graphic throws NRE and ApplyModifiedProperties never runs, so the field stays empty.
    /// </summary>
    internal static class SelectableTargetGraphicEditorFix
    {
        public static void DrawWithNreGuard(UnityEditor.Editor editor, Action drawInspector)
        {
            try
            {
                drawInspector();
            }
            catch (NullReferenceException)
            {
                editor.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    [CustomEditor(typeof(Button), true)]
    [CanEditMultipleObjects]
    public sealed class ButtonEditorNreFix : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            SelectableTargetGraphicEditorFix.DrawWithNreGuard(this, () => base.OnInspectorGUI());
        }
    }

    [CustomEditor(typeof(Toggle), true)]
    [CanEditMultipleObjects]
    public sealed class ToggleEditorNreFix : ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            SelectableTargetGraphicEditorFix.DrawWithNreGuard(this, () => base.OnInspectorGUI());
        }
    }

    [CustomEditor(typeof(Slider), true)]
    [CanEditMultipleObjects]
    public sealed class SliderEditorNreFix : SliderEditor
    {
        public override void OnInspectorGUI()
        {
            SelectableTargetGraphicEditorFix.DrawWithNreGuard(this, () => base.OnInspectorGUI());
        }
    }

    [CustomEditor(typeof(Scrollbar), true)]
    [CanEditMultipleObjects]
    public sealed class ScrollbarEditorNreFix : ScrollbarEditor
    {
        public override void OnInspectorGUI()
        {
            SelectableTargetGraphicEditorFix.DrawWithNreGuard(this, () => base.OnInspectorGUI());
        }
    }

    [CustomEditor(typeof(Dropdown), true)]
    [CanEditMultipleObjects]
    public sealed class DropdownEditorNreFix : DropdownEditor
    {
        public override void OnInspectorGUI()
        {
            SelectableTargetGraphicEditorFix.DrawWithNreGuard(this, () => base.OnInspectorGUI());
        }
    }

    [CustomEditor(typeof(InputField), true)]
    [CanEditMultipleObjects]
    public sealed class InputFieldEditorNreFix : InputFieldEditor
    {
        public override void OnInspectorGUI()
        {
            SelectableTargetGraphicEditorFix.DrawWithNreGuard(this, () => base.OnInspectorGUI());
        }
    }
}
#endif
