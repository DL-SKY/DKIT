using System;
using UnityEditor;
using UnityEngine;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class IdentifierPromptWindow : EditorWindow
    {
        private Action<string> _onConfirm;
        private string _value;
        private string _confirmButtonText;

        public static void Open(string title, string initialValue, string confirmButtonText, Action<string> onConfirm)
        {
            IdentifierPromptWindow window = CreateInstance<IdentifierPromptWindow>();
            window.titleContent = new GUIContent(title);
            window._value = initialValue ?? string.Empty;
            window._confirmButtonText = string.IsNullOrWhiteSpace(confirmButtonText) ? "OK" : confirmButtonText;
            window._onConfirm = onConfirm;
            window.minSize = new Vector2(420f, 92f);
            window.maxSize = new Vector2(420f, 92f);
            window.ShowUtility();
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            GUI.SetNextControlName("id-field");
            _value = EditorGUILayout.TextField("Id", _value ?? string.Empty);
            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Cancel", GUILayout.Width(90f)))
                {
                    Close();
                    return;
                }

                if (GUILayout.Button(_confirmButtonText, GUILayout.Width(90f)))
                {
                    ConfirmAndClose();
                    return;
                }
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    ConfirmAndClose();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    Event.current.Use();
                }
            }
        }

        private void OnFocus()
        {
            EditorApplication.delayCall += FocusInput;
        }

        private void FocusInput()
        {
            if (this == null)
                return;

            EditorGUI.FocusTextInControl("id-field");
        }

        private void ConfirmAndClose()
        {
            _onConfirm?.Invoke(_value);
            Close();
        }
    }
}
