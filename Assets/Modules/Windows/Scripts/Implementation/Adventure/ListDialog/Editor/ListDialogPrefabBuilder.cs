#if UNITY_EDITOR
using Modules.Windows.Scripts.Components;
using Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Items;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog.Editor
{
    /// <summary>
    /// Builds ListDialog shell and item prefabs.
    /// Shell from TemplateView; items are plain UI widgets (no Canvas).
    /// Does not create or edit .meta files.
    /// </summary>
    public static class ListDialogPrefabBuilder
    {
        private const string TemplatePath = "Assets/ResourcesNotInclude/Cursor/TemplateView.prefab";

        private const string RootFolder =
            "Assets/Modules/Windows/Resources/Prefabs/Views/Adventure/ListDialog";

        private const string ItemsFolder = RootFolder + "/Items";

        [MenuItem("Tools/Cursor/Build List Dialog Prefabs")]
        public static void BuildAll()
        {
            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath);
            if (template == null)
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(ListDialogPrefabBuilder)}] Template not found: {TemplatePath}");
                return;
            }

            EnsureFolder(RootFolder);
            EnsureFolder(ItemsFolder);

            BuildShellView(template);
            BuildDefinitionItemView();
            BuildAvatarItemView();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log(
                $"[{nameof(ListDialogPrefabBuilder)}] Prefabs built in {RootFolder}");
        }

        private static void BuildShellView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "ListDialogView");
            Transform contentRoot = GetOrCreateContentRoot(root);
            var view = root.AddComponent<ListDialogView>();

            VerticalLayoutGroup layout = EnsureVerticalLayout(contentRoot.gameObject);
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;

            TextMeshProUGUI title = CreateTmp(contentRoot, "Title", "Выбор", TextAlignmentOptions.Center, 26f);
            SetPreferredHeight(title.gameObject, 36f);

            RectTransform scrollRoot = CreateRow(contentRoot, "Scroll", 400f);
            ScrollRect scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scrollRoot.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.6f);

            RectTransform viewport = CreateRow(scrollRoot, "Viewport", 0f);
            StretchFull(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);

            RectTransform listContent = CreateRow(viewport, "Content", 0f);
            listContent.anchorMin = new Vector2(0f, 1f);
            listContent.anchorMax = new Vector2(1f, 1f);
            listContent.pivot = new Vector2(0.5f, 1f);
            listContent.sizeDelta = Vector2.zero;

            VerticalLayoutGroup contentLayout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            listContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = listContent;

            Button cancel = CreateButton(contentRoot, "CancelButton", "Отмена", 44f);
            SetPreferredHeight(cancel.gameObject, 44f);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_content").objectReferenceValue = listContent;
            so.FindProperty("_cancelButton").objectReferenceValue = cancel;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, RootFolder, "ListDialogView.prefab");
        }

        private static void BuildDefinitionItemView()
        {
            GameObject root = new GameObject(
                "DefinitionListDialogItemView",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button button = root.GetComponent<Button>();
            button.targetGraphic = image;

            LayoutElement rootLe = root.GetComponent<LayoutElement>();
            rootLe.preferredHeight = 56f;
            rootLe.minHeight = 56f;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 6, 6);
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateTmp(root.transform, "Title", "Title", TextAlignmentOptions.Left, 18f);
            SetPreferredHeight(title.gameObject, 24f);

            TextMeshProUGUI description =
                CreateTmp(root.transform, "Description", "Description", TextAlignmentOptions.Left, 14f);
            description.color = new Color(1f, 1f, 1f, 0.7f);
            SetPreferredHeight(description.gameObject, 20f);

            var view = root.AddComponent<DefinitionListDialogItemView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_description").objectReferenceValue = description;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ItemsFolder, "DefinitionListDialogItemView.prefab");
        }

        private static void BuildAvatarItemView()
        {
            GameObject root = new GameObject(
                "AvatarListDialogItemView",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            Button button = root.GetComponent<Button>();
            button.targetGraphic = image;

            LayoutElement rootLe = root.GetComponent<LayoutElement>();
            rootLe.preferredWidth = 96f;
            rootLe.preferredHeight = 112f;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            CachedPathImage avatar = CreateCachedImage(root.transform, "Avatar", 72f, 72f);
            TextMeshProUGUI title = CreateTmp(root.transform, "Title", "Avatar", TextAlignmentOptions.Center, 14f);
            SetPreferredHeight(title.gameObject, 22f);

            var view = root.AddComponent<AvatarListDialogItemView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_button").objectReferenceValue = button;
            so.FindProperty("_avatarImage").objectReferenceValue = avatar;
            so.FindProperty("_title").objectReferenceValue = title;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ItemsFolder, "AvatarListDialogItemView.prefab");
        }

        private static GameObject InstantiateFromTemplate(GameObject template, string viewName)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(template);
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

        private static Transform GetOrCreateContentRoot(GameObject root)
        {
            Transform existing = root.transform.Find("Content");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            Transform background = root.transform.Find("Background");
            if (background != null)
            {
                for (int i = background.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(background.GetChild(i).gameObject);
            }

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.layer = root.layer;
            content.transform.SetParent(root.transform, false);

            if (background != null)
                content.transform.SetSiblingIndex(background.GetSiblingIndex() + 1);

            RectTransform rt = content.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            return content.transform;
        }

        private static VerticalLayoutGroup EnsureVerticalLayout(GameObject go)
        {
            VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
                layout = go.AddComponent<VerticalLayoutGroup>();
            return layout;
        }

        private static RectTransform CreateRow(Transform parent, string name, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            if (height > 0f)
                SetPreferredHeight(go, height);
            return go.GetComponent<RectTransform>();
        }

        private static Button CreateButton(Transform parent, string name, string text, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.28f, 0.35f, 0.5f, 1f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;

            TextMeshProUGUI label = CreateTmp(go.transform, "Text", text, TextAlignmentOptions.Center, 18f);
            StretchInsets(label.rectTransform, 4f, 4f, 4f, 4f);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            return button;
        }

        private static TextMeshProUGUI CreateTmp(
            Transform parent,
            string name,
            string text,
            TextAlignmentOptions alignment,
            float fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;
            return tmp;
        }

        private static CachedPathImage CreateCachedImage(Transform parent, string name, float width, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CachedPathImage));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = Color.white;
            image.enabled = false;

            CachedPathImage cached = go.GetComponent<CachedPathImage>();
            SerializedObject so = new SerializedObject(cached);
            so.FindProperty("_image").objectReferenceValue = image;
            so.ApplyModifiedPropertiesWithoutUndo();

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;
            le.minWidth = width;
            le.minHeight = height;
            return cached;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void StretchInsets(RectTransform rt, float left, float right, float top, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetPreferredHeight(GameObject go, float height)
        {
            LayoutElement le = go.GetComponent<LayoutElement>();
            if (le == null)
                le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
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
