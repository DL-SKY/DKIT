using UnityEditor;

namespace Modules.Cheats.Scripts.Editor.Core
{
    public abstract class CheatSectionBase
    {
        public abstract string Id { get; }

        public virtual int Order => 0;

        public virtual bool IsVisible(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            string source = Id.ToLowerInvariant();
            string[] tokens = filter.ToLowerInvariant().Split(' ');
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (!source.Contains(token))
                {
                    return false;
                }
            }

            return true;
        }

        public abstract void DrawContent();

        protected void DrawSectionHeader(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        protected void DrawHelpInfo(string text)
        {
            EditorGUILayout.HelpBox(text, MessageType.Info);
        }
    }
}
