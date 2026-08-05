#if UNITY_EDITOR
using Modules.Windows.Scripts.Components;
using Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.Editor
{
    /// <summary>
    /// Builds Cursor* character-creation prefabs from
    /// <c>Assets/ResourcesNotInclude/Cursor/TemplateView.prefab</c>.
    /// Output: <see cref="RootFolder"/>. Does not create or edit .meta files.
    /// </summary>
    public static class CursorCreateCharacterPrefabBuilder
    {
        private const string TemplatePath = "Assets/ResourcesNotInclude/Cursor/TemplateView.prefab";

        private const string RootFolder =
            "Assets/Modules/Windows/Resources/Prefabs/Views/Adventure/Characters/CursorCreateCharacter";

        [MenuItem("Tools/Cursor/Build Create Character Prefabs")]
        public static void BuildAll()
        {
            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath);
            if (template == null)
            {
                UnityEngine.Debug.LogError(
                    $"[{nameof(CursorCreateCharacterPrefabBuilder)}] Template not found: {TemplatePath}");
                return;
            }

            EnsureFolder(RootFolder);

            BuildMainView(template);
            BuildEditNameView(template);
            BuildSelectDefinitionView(template);
            BuildDistributeAbilitiesView(template);
            BuildSelectSkillsView(template);
            BuildSelectAvatarView(template);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log($"[{nameof(CursorCreateCharacterPrefabBuilder)}] Prefabs built in {RootFolder}");
        }

        private static void BuildMainView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "CursorCreateCharacterView");
            Transform content = GetOrCreateContentRoot(root);
            var view = root.AddComponent<CursorCreateCharacterView>();

            VerticalLayoutGroup layout = EnsureVerticalLayout(content.gameObject);
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 10f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            CachedPathImage avatarImage = CreateCachedImage(content, "Avatar", 120f, 120f);
            Button editAvatar = CreateButton(content, "EditAvatarButton", "Изменить аватар", 36f);

            TextMeshProUGUI nameValue;
            Button editName;
            CreateLabeledRow(content, "NameRow", "Имя", out nameValue, out editName);

            TextMeshProUGUI ancestryValue;
            Button editAncestry;
            CreateLabeledRow(content, "AncestryRow", "Происхождение", out ancestryValue, out editAncestry);

            TextMeshProUGUI classValue;
            Button editClass;
            CreateLabeledRow(content, "ClassRow", "Класс", out classValue, out editClass);

            TextMeshProUGUI backgroundValue;
            Button editBackground;
            CreateLabeledRow(content, "BackgroundRow", "Предыстория", out backgroundValue, out editBackground);

            RectTransform abilitiesHeader = CreateRow(content, "AbilitiesHeader", 40f);
            HorizontalLayoutGroup abilitiesHeaderLayout =
                abilitiesHeader.gameObject.AddComponent<HorizontalLayoutGroup>();
            abilitiesHeaderLayout.childAlignment = TextAnchor.MiddleLeft;
            abilitiesHeaderLayout.childForceExpandWidth = false;
            abilitiesHeaderLayout.spacing = 8f;

            TextMeshProUGUI abilitiesTitle =
                CreateTmp(abilitiesHeader, "Title", "Характеристики", TextAlignmentOptions.Left, 22f);
            abilitiesTitle.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Button editAbilities = CreateButton(abilitiesHeader, "EditAbilitiesButton", "Распределить", 32f);
            editAbilities.GetComponent<LayoutElement>().preferredWidth = 160f;

            TextMeshProUGUI boostPoints =
                CreateTmp(content, "BoostPoints", "Свободно: 0", TextAlignmentOptions.Left, 18f);
            SetPreferredHeight(boostPoints.gameObject, 28f);

            RectTransform abilitiesRow = CreateRow(content, "AbilitiesValues", 72f);
            HorizontalLayoutGroup abilitiesLayout = abilitiesRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            abilitiesLayout.spacing = 8f;
            abilitiesLayout.childAlignment = TextAnchor.MiddleCenter;
            abilitiesLayout.childForceExpandWidth = true;
            abilitiesLayout.childForceExpandHeight = true;

            var abilityIcons = new CachedPathImage[6];
            var abilityValues = new TextMeshProUGUI[6];
            string[] abilityNames = { "STR", "DEX", "CON", "INT", "WIS", "CHA" };
            for (int i = 0; i < 6; i++)
            {
                RectTransform cell = CreateRow(abilitiesRow, abilityNames[i], 64f);
                VerticalLayoutGroup cellLayout = cell.gameObject.AddComponent<VerticalLayoutGroup>();
                cellLayout.childAlignment = TextAnchor.MiddleCenter;
                cellLayout.spacing = 4f;

                abilityIcons[i] = CreateCachedImage(cell, "Icon", 36f, 36f);
                abilityValues[i] = CreateTmp(cell, "Value", "0", TextAlignmentOptions.Center, 18f);
            }

            Button editSkills = CreateButton(content, "EditSkillsButton", "Навыки", 40f);
            SetPreferredHeight(editSkills.gameObject, 40f);

            RectTransform footer = CreateRow(content, "Footer", 48f);
            HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 12f;
            footerLayout.childForceExpandWidth = true;
            footerLayout.childForceExpandHeight = true;

            Button closeButton = CreateButton(footer, "CloseButton", "Закрыть", 44f);
            Button createActive = CreateButton(footer, "CreateActiveButton", "Создать", 44f);
            Button createDisable = CreateButton(footer, "CreateDisableButton", "Создать (не готово)", 44f);
            createDisable.gameObject.SetActive(false);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_avatarImage").objectReferenceValue = avatarImage;
            so.FindProperty("_editAvatarButton").objectReferenceValue = editAvatar;
            so.FindProperty("_nameValue").objectReferenceValue = nameValue;
            so.FindProperty("_editNameButton").objectReferenceValue = editName;
            so.FindProperty("_ancestryValue").objectReferenceValue = ancestryValue;
            so.FindProperty("_editAncestryButton").objectReferenceValue = editAncestry;
            so.FindProperty("_classValue").objectReferenceValue = classValue;
            so.FindProperty("_editClassButton").objectReferenceValue = editClass;
            so.FindProperty("_backgroundValue").objectReferenceValue = backgroundValue;
            so.FindProperty("_editBackgroundButton").objectReferenceValue = editBackground;
            so.FindProperty("_editAbilitiesButton").objectReferenceValue = editAbilities;
            so.FindProperty("_boostPointsValue").objectReferenceValue = boostPoints;
            so.FindProperty("_editSkillsButton").objectReferenceValue = editSkills;
            so.FindProperty("_closeButton").objectReferenceValue = closeButton;
            so.FindProperty("_createActiveButton").objectReferenceValue = createActive;
            so.FindProperty("_createDisableButton").objectReferenceValue = createDisable;

            SerializedProperty iconsProp = so.FindProperty("_abilityIcons");
            iconsProp.arraySize = 6;
            SerializedProperty valuesProp = so.FindProperty("_abilityValues");
            valuesProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                iconsProp.GetArrayElementAtIndex(i).objectReferenceValue = abilityIcons[i];
                valuesProp.GetArrayElementAtIndex(i).objectReferenceValue = abilityValues[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab(root, "CursorCreateCharacterView.prefab");
        }

        private static void BuildEditNameView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "CursorEditNameView");
            Transform content = GetOrCreateContentRoot(root);
            var view = root.AddComponent<CursorEditNameView>();

            VerticalLayoutGroup layout = EnsureVerticalLayout(content.gameObject);
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            CreateTmp(content, "Title", "Имя персонажа", TextAlignmentOptions.Center, 26f);

            TMP_InputField input = CreateInputField(content, "NameInput");
            SetPreferredHeight(input.gameObject, 44f);

            RectTransform footer = CreateRow(content, "Footer", 44f);
            HorizontalLayoutGroup footerLayout = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 12f;
            footerLayout.childForceExpandWidth = true;
            footerLayout.childForceExpandHeight = true;

            Button cancel = CreateButton(footer, "CancelButton", "Отмена", 40f);
            Button confirm = CreateButton(footer, "ConfirmButton", "ОК", 40f);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_nameInput").objectReferenceValue = input;
            so.FindProperty("_confirmButton").objectReferenceValue = confirm;
            so.FindProperty("_cancelButton").objectReferenceValue = cancel;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "CursorEditNameView.prefab");
        }

        private static void BuildSelectDefinitionView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "CursorSelectDefinitionView");
            Transform contentRoot = GetOrCreateContentRoot(root);
            var view = root.AddComponent<CursorSelectDefinitionView>();

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

            Button itemTemplate = CreateButton(listContent, "ItemTemplate", "Item", 48f);
            itemTemplate.gameObject.SetActive(false);

            Button cancel = CreateButton(contentRoot, "CancelButton", "Отмена", 44f);
            SetPreferredHeight(cancel.gameObject, 44f);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_title").objectReferenceValue = title;
            so.FindProperty("_content").objectReferenceValue = listContent;
            so.FindProperty("_itemTemplate").objectReferenceValue = itemTemplate;
            so.FindProperty("_cancelButton").objectReferenceValue = cancel;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "CursorSelectDefinitionView.prefab");
        }

        private static void BuildDistributeAbilitiesView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "CursorDistributeAbilitiesView");
            Transform contentRoot = GetOrCreateContentRoot(root);
            var view = root.AddComponent<CursorDistributeAbilitiesView>();

            VerticalLayoutGroup layout = EnsureVerticalLayout(contentRoot.gameObject);
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            CreateTmp(contentRoot, "Title", "Распределение характеристик", TextAlignmentOptions.Center, 24f);
            TextMeshProUGUI boost =
                CreateTmp(contentRoot, "BoostPoints", "Свободные очки: 0", TextAlignmentOptions.Center, 20f);
            SetPreferredHeight(boost.gameObject, 28f);

            RectTransform listContent = CreateRow(contentRoot, "Content", 360f);
            VerticalLayoutGroup contentLayout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childControlHeight = true;

            GameObject rowTemplate = CreateAbilityRowTemplate(listContent);
            rowTemplate.SetActive(false);

            Button close = CreateButton(contentRoot, "CloseButton", "Готово", 44f);
            SetPreferredHeight(close.gameObject, 44f);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_boostPointsLabel").objectReferenceValue = boost;
            so.FindProperty("_content").objectReferenceValue = listContent;
            so.FindProperty("_rowTemplate").objectReferenceValue = rowTemplate;
            so.FindProperty("_closeButton").objectReferenceValue = close;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "CursorDistributeAbilitiesView.prefab");
        }

        private static void BuildSelectSkillsView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "CursorSelectSkillsView");
            Transform contentRoot = GetOrCreateContentRoot(root);
            var view = root.AddComponent<CursorSelectSkillsView>();

            VerticalLayoutGroup layout = EnsureVerticalLayout(contentRoot.gameObject);
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            CreateTmp(contentRoot, "Title", "Навыки (степень изученности)", TextAlignmentOptions.Center, 24f);

            RectTransform scrollRoot = CreateRow(contentRoot, "Scroll", 420f);
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
            VerticalLayoutGroup contentLayout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childForceExpandWidth = true;
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            listContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = listContent;

            GameObject rowTemplate = CreateSkillRowTemplate(listContent);
            rowTemplate.SetActive(false);

            Button close = CreateButton(contentRoot, "CloseButton", "Готово", 44f);
            SetPreferredHeight(close.gameObject, 44f);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_content").objectReferenceValue = listContent;
            so.FindProperty("_rowTemplate").objectReferenceValue = rowTemplate;
            so.FindProperty("_closeButton").objectReferenceValue = close;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "CursorSelectSkillsView.prefab");
        }

        private static void BuildSelectAvatarView(GameObject template)
        {
            GameObject root = InstantiateFromTemplate(template, "CursorSelectAvatarView");
            Transform contentRoot = GetOrCreateContentRoot(root);
            var view = root.AddComponent<CursorSelectAvatarView>();

            VerticalLayoutGroup layout = EnsureVerticalLayout(contentRoot.gameObject);
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            CreateTmp(contentRoot, "Title", "Аватар", TextAlignmentOptions.Center, 24f);

            RectTransform listContent = CreateRow(contentRoot, "Content", 160f);
            VerticalLayoutGroup contentLayout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8f;
            contentLayout.childForceExpandWidth = true;

            Button itemTemplate = CreateButton(listContent, "ItemTemplate", "Avatar", 48f);
            itemTemplate.gameObject.SetActive(false);

            Button cancel = CreateButton(contentRoot, "CancelButton", "Отмена", 44f);
            SetPreferredHeight(cancel.gameObject, 44f);

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_content").objectReferenceValue = listContent;
            so.FindProperty("_itemTemplate").objectReferenceValue = itemTemplate;
            so.FindProperty("_cancelButton").objectReferenceValue = cancel;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "CursorSelectAvatarView.prefab");
        }

        /// <summary>
        /// Instantiates <paramref name="template"/>, unpacks it, renames root.
        /// Keeps Canvas / GraphicRaycaster / Background from TemplateView.
        /// </summary>
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

        /// <summary>
        /// Content is a sibling of Background under the view root.
        /// Background stays empty and oversized (sizeDelta overhang) so it covers notches;
        /// do not parent Content under Background.
        /// Matches CursorCreateCharacterView.prefab layout:
        /// Root → Background, Content (stretch, sizeDelta 0).
        /// </summary>
        private static Transform GetOrCreateContentRoot(GameObject root)
        {
            Transform existing = root.transform.Find("Content");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            // Keep Background as first sibling; ensure it has no Content children.
            Transform background = root.transform.Find("Background");
            if (background != null)
            {
                for (int i = background.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(background.GetChild(i).gameObject);
            }

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.layer = root.layer;
            content.transform.SetParent(root.transform, false);

            // Place after Background so draw order is: Background → Content.
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

        private static GameObject CreateAbilityRowTemplate(Transform parent)
        {
            RectTransform row = CreateRow(parent, "RowTemplate", 48f);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.padding = new RectOffset(8, 8, 4, 4);
            row.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            CreateCachedImage(row, "Icon", 32f, 32f);
            TextMeshProUGUI title = CreateTmp(row, "Title", "STR", TextAlignmentOptions.Left, 18f);
            LayoutElement titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;
            titleLe.minWidth = 80f;

            CreateButton(row, "DecreaseButton", "-", 40f).GetComponent<LayoutElement>().preferredWidth = 40f;
            TextMeshProUGUI value = CreateTmp(row, "Value", "0", TextAlignmentOptions.Center, 18f);
            LayoutElement valueLe = value.gameObject.AddComponent<LayoutElement>();
            valueLe.preferredWidth = 48f;
            CreateButton(row, "IncreaseButton", "+", 40f).GetComponent<LayoutElement>().preferredWidth = 40f;

            return row.gameObject;
        }

        private static GameObject CreateSkillRowTemplate(Transform parent)
        {
            RectTransform row = CreateRow(parent, "RowTemplate", 48f);
            Button button = row.gameObject.AddComponent<Button>();
            row.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
            button.targetGraphic = row.GetComponent<Image>();

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childAlignment = TextAnchor.MiddleCenter;

            CreateCachedImage(row, "Icon", 32f, 32f);
            TextMeshProUGUI title = CreateTmp(row, "Title", "Skill", TextAlignmentOptions.Left, 18f);
            title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            CreateTmp(row, "Rank", "Untrained", TextAlignmentOptions.Right, 18f)
                .gameObject.AddComponent<LayoutElement>().preferredWidth = 120f;

            return row.gameObject;
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

        private static void CreateLabeledRow(
            Transform parent,
            string rowName,
            string label,
            out TextMeshProUGUI value,
            out Button editButton)
        {
            RectTransform row = CreateRow(parent, rowName, 40f);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            TextMeshProUGUI title = CreateTmp(row, "Label", label + ":", TextAlignmentOptions.Left, 20f);
            LayoutElement titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.preferredWidth = 180f;

            value = CreateTmp(row, "Value", "—", TextAlignmentOptions.Left, 20f);
            LayoutElement valueLe = value.gameObject.AddComponent<LayoutElement>();
            valueLe.flexibleWidth = 1f;

            editButton = CreateButton(row, "EditButton", "…", 36f);
            LayoutElement buttonLe = editButton.gameObject.GetComponent<LayoutElement>();
            if (buttonLe == null)
                buttonLe = editButton.gameObject.AddComponent<LayoutElement>();
            buttonLe.preferredWidth = 48f;
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

        private static TMP_InputField CreateInputField(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.08f, 0.08f, 0.1f, 1f);

            GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.layer = go.layer;
            textArea.transform.SetParent(go.transform, false);
            RectTransform textAreaRt = textArea.GetComponent<RectTransform>();
            StretchInsets(textAreaRt, 8f, 8f, 8f, 8f);

            TextMeshProUGUI text = CreateTmp(textArea.transform, "Text", string.Empty, TextAlignmentOptions.Left, 20f);
            StretchFull(text.rectTransform);

            TextMeshProUGUI placeholder = CreateTmp(
                textArea.transform, "Placeholder", "Введите имя…", TextAlignmentOptions.Left, 20f);
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(1f, 1f, 1f, 0.45f);
            StretchFull(placeholder.rectTransform);

            TMP_InputField input = go.GetComponent<TMP_InputField>();
            input.textViewport = textAreaRt;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = TMP_Settings.defaultFontAsset;
            return input;
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

        private static void SavePrefab(GameObject root, string fileName)
        {
            string path = $"{RootFolder}/{fileName}";
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
