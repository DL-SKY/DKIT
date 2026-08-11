#if UNITY_EDITOR
using Modules.Localization.Scripts.Components;
using Modules.Windows.Scripts.Managers;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Hints.Editor
{
    /// <summary>
    /// Builds HintView prefab and wires HintManager onto the WindowsManager prefab.
    /// Does not create or edit .meta files.
    /// </summary>
    public static class HintPrefabBuilder
    {
        private const string TemplatePath = "Assets/ResourcesNotInclude/Cursor/TemplateView.prefab";

        private const string HintFolder =
            "Assets/Modules/Windows/Resources/Prefabs/Views/Hints";

        private const string WindowsManagerPrefabPath =
            "Assets/Modules/Windows/Resources/Prefabs/Main/WindowsManager.prefab";

        private const float PanelWidth = 720f;
        private const float PanelTopOffset = 120f;

        [MenuItem("Tools/Cursor/Build Hint Prefabs")]
        public static void BuildAll()
        {
            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath);
            if (template == null)
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(HintPrefabBuilder)}] Template not found: {TemplatePath}");
                return;
            }

            EnsureFolder(HintFolder);
            BuildHintView(template);
            WireHintManagerOnWindowsManager();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log(
                $"[{nameof(HintPrefabBuilder)}] HintView built in {HintFolder}; HintManager wired on WindowsManager.");
        }

        [MenuItem("Tools/Cursor/Wire HintManager On WindowsManager")]
        public static void WireHintManagerOnWindowsManager()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(WindowsManagerPrefabPath);
            if (root == null)
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(HintPrefabBuilder)}] WindowsManager prefab not found: {WindowsManagerPrefabPath}");
                return;
            }

            try
            {
                if (root.GetComponent<HintManager>() == null)
                    root.AddComponent<HintManager>();

                PrefabUtility.SaveAsPrefabAsset(root, WindowsManagerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BuildHintView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "HintView");

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Transform background = root.transform.Find("Background");
            if (background != null)
            {
                Image bgImage = background.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = new Color(0f, 0f, 0f, 0f);
                    bgImage.raycastTarget = false;
                }
            }

            RectTransform panel = CreatePanel(root.transform);
            LocalizationText localizationText = CreateHintText(panel);

            HintView view = root.GetComponent<HintView>();
            if (view == null)
                view = root.AddComponent<HintView>();

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("_text").objectReferenceValue = localizationText;
            so.FindProperty("_showAnimationSeconds").floatValue = 0.25f;
            so.FindProperty("_hideAnimationSeconds").floatValue = 0.25f;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, HintFolder, "HintView.prefab");
        }

        private static RectTransform CreatePanel(Transform parent)
        {
            GameObject panelGo = new GameObject(
                "Panel",
                typeof(RectTransform),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            panelGo.layer = parent.gameObject.layer;
            panelGo.transform.SetParent(parent, false);

            RectTransform rt = panelGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -PanelTopOffset);
            rt.sizeDelta = new Vector2(PanelWidth, 0f);

            Image image = panelGo.GetComponent<Image>();
            image.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            image.raycastTarget = false;

            VerticalLayoutGroup layout = panelGo.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 16, 16);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = panelGo.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rt;
        }

        private static LocalizationText CreateHintText(Transform parent)
        {
            GameObject go = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(LocalizationText));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = "Hint";
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;

            LayoutElement layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.minWidth = PanelWidth - 48f;
            layoutElement.preferredWidth = PanelWidth - 48f;

            return go.GetComponent<LocalizationText>();
        }

        private static GameObject InstantiateFromTemplate(GameObject template, string viewName)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(template) as GameObject;
            if (instance == null)
                instance = Object.Instantiate(template);

            if (PrefabUtility.IsPartOfPrefabInstance(instance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    instance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            instance.name = viewName;
            instance.layer = template.layer;
            return instance;
        }

        private static void SavePrefab(GameObject root, string folder, string fileName)
        {
            string path = $"{folder}/{fileName}";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
                return;

            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }

            string absolute = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetFolder));
            if (!Directory.Exists(absolute))
                Directory.CreateDirectory(absolute);
        }
    }
}
#endif
