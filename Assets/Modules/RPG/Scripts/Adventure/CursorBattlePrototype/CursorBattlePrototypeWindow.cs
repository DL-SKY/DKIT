using Modules.Windows.Scripts.Implementation.Adventure.CursorBattlePrototype;
using Modules.Windows.Scripts.Base;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.RPG.Scripts.Adventure.CursorBattlePrototype
{
    /// <summary>
    /// uGUI view for combat prototype (used by prefab).
    /// </summary>
    public sealed class CursorBattlePrototypeWindow : ViewBase<CursorBattlePrototypeViewModel>
    {
        [Header("Main Text")]
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _partyText;
        [SerializeField] private TextMeshProUGUI _enemyText;
        [SerializeField] private TextMeshProUGUI _actorText;
        [SerializeField] private TextMeshProUGUI _actionText;
        [SerializeField] private TextMeshProUGUI _targetText;
        [SerializeField] private TextMeshProUGUI _logText;

        [Header("Control Buttons")]
        [SerializeField] private Button _startDemoButton;
        [SerializeField] private Button _applyActionButton;
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private Button _closeButton;

        [Header("Actor Selector")]
        [SerializeField] private Button _prevActorButton;
        [SerializeField] private Button _nextActorButton;

        [Header("Action Selector")]
        [SerializeField] private Button _prevActionButton;
        [SerializeField] private Button _nextActionButton;

        [Header("Target Selector")]
        [SerializeField] private Button _prevTargetButton;
        [SerializeField] private Button _nextTargetButton;

        public static string Path = "Prefabs/Views/Adventure/CursorBattlePrototype/CursorBattlePrototypeWindow";

        protected override void InitImplementation()
        {
            AutoWireIfNeeded();
            ApplyAll();
        }

        protected override void Subscribe()
        {
            // ViewBase calls Subscribe before InitImplementation, so ensure controls exist first.
            AutoWireIfNeeded();
            _viewModel.OnChangeCustom += OnChangeCustomHandler;
            Bind(_startDemoButton, OnStartDemoButtonClick);
            Bind(_applyActionButton, OnApplyActionButtonClick);
            Bind(_endTurnButton, OnEndTurnButtonClick);
            Bind(_closeButton, OnCloseButtonClick);

            Bind(_prevActorButton, OnPrevActorButtonClick);
            Bind(_nextActorButton, OnNextActorButtonClick);
            Bind(_prevActionButton, OnPrevActionButtonClick);
            Bind(_nextActionButton, OnNextActionButtonClick);
            Bind(_prevTargetButton, OnPrevTargetButtonClick);
            Bind(_nextTargetButton, OnNextTargetButtonClick);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            Unbind(_startDemoButton, OnStartDemoButtonClick);
            Unbind(_applyActionButton, OnApplyActionButtonClick);
            Unbind(_endTurnButton, OnEndTurnButtonClick);
            Unbind(_closeButton, OnCloseButtonClick);

            Unbind(_prevActorButton, OnPrevActorButtonClick);
            Unbind(_nextActorButton, OnNextActorButtonClick);
            Unbind(_prevActionButton, OnPrevActionButtonClick);
            Unbind(_nextActionButton, OnNextActionButtonClick);
            Unbind(_prevTargetButton, OnPrevTargetButtonClick);
            Unbind(_nextTargetButton, OnNextTargetButtonClick);
        }

        private void Update()
        {
            _viewModel?.Tick();
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == CursorBattlePrototypeViewModel.ON_CHANGE_ALL)
                ApplyAll();
        }

        private void ApplyAll()
        {
            if (_viewModel == null)
                return;

            SetText(_headerText, _viewModel.HeaderText);
            SetText(_partyText, _viewModel.PartySummaryText);
            SetText(_enemyText, _viewModel.EnemySummaryText);
            SetText(_actorText, _viewModel.ActorSelectorText);
            SetText(_actionText, _viewModel.ActionSelectorText);
            SetText(_targetText, _viewModel.TargetSelectorText);
            SetText(_logText, _viewModel.LogText);

            bool canInteractTurn = _viewModel.HasSession && _viewModel.IsPlayerTurn;
            SetInteractable(_applyActionButton, canInteractTurn);
            SetInteractable(_endTurnButton, _viewModel.HasSession);
            SetInteractable(_prevActorButton, canInteractTurn);
            SetInteractable(_nextActorButton, canInteractTurn);
            SetInteractable(_prevActionButton, canInteractTurn);
            SetInteractable(_nextActionButton, canInteractTurn);
            SetInteractable(_prevTargetButton, canInteractTurn);
            SetInteractable(_nextTargetButton, canInteractTurn);

            if (_startDemoButton != null)
                _startDemoButton.gameObject.SetActive(!_viewModel.HasSession);
        }

        private void OnStartDemoButtonClick() => _viewModel?.OnStartDemoEncounter();
        private void OnApplyActionButtonClick() => _viewModel?.OnApplyAction();
        private void OnEndTurnButtonClick() => _viewModel?.OnEndTurn();
        private void OnCloseButtonClick() => _viewModel?.OnClose();
        private void OnPrevActorButtonClick() => _viewModel?.OnPrevActor();
        private void OnNextActorButtonClick() => _viewModel?.OnNextActor();
        private void OnPrevActionButtonClick() => _viewModel?.OnPrevAction();
        private void OnNextActionButtonClick() => _viewModel?.OnNextAction();
        private void OnPrevTargetButtonClick() => _viewModel?.OnPrevTarget();
        private void OnNextTargetButtonClick() => _viewModel?.OnNextTarget();

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
                text.text = value ?? string.Empty;
        }

        private static void SetInteractable(Button button, bool value)
        {
            if (button != null)
                button.interactable = value;
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null && action != null)
                button.onClick.AddListener(action);
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null && action != null)
                button.onClick.RemoveListener(action);
        }

        private void AutoWireIfNeeded()
        {
            if (_headerText == null)
                BuildDefaultUi();

            _headerText ??= FindTmp("HeaderText");
            _partyText ??= FindTmp("PartyText");
            _enemyText ??= FindTmp("EnemyText");
            _actorText ??= FindTmp("ActorText");
            _actionText ??= FindTmp("ActionText");
            _targetText ??= FindTmp("TargetText");
            _logText ??= FindTmp("LogText");

            _startDemoButton ??= FindButton("StartDemoButton");
            _applyActionButton ??= FindButton("ApplyActionButton");
            _endTurnButton ??= FindButton("EndTurnButton");
            _closeButton ??= FindButton("CloseButton");

            _prevActorButton ??= FindButton("PrevActorButton");
            _nextActorButton ??= FindButton("NextActorButton");
            _prevActionButton ??= FindButton("PrevActionButton");
            _nextActionButton ??= FindButton("NextActionButton");
            _prevTargetButton ??= FindButton("PrevTargetButton");
            _nextTargetButton ??= FindButton("NextTargetButton");
        }

        private void BuildDefaultUi()
        {
            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null)
                rootRect = gameObject.AddComponent<RectTransform>();

            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(12f, 12f);
            rootRect.offsetMax = new Vector2(-12f, -12f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            CanvasRenderer rootCanvasRenderer = GetComponent<CanvasRenderer>();
            if (rootCanvasRenderer == null)
                gameObject.AddComponent<CanvasRenderer>();

            Image rootImage = GetComponent<Image>();
            if (rootImage == null)
                rootImage = gameObject.AddComponent<Image>();
            rootImage.color = new Color(0.08f, 0.08f, 0.1f, 0.96f);

            VerticalLayoutGroup rootLayout = GetComponent<VerticalLayoutGroup>();
            if (rootLayout == null)
                rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(16, 16, 16, 16);
            rootLayout.spacing = 12f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = false;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            _headerText = CreateText(transform, "HeaderText", 26, FontStyles.Bold, TextAlignmentOptions.Center);
            _headerText.enableWordWrapping = true;
            SetPreferredHeight(_headerText.gameObject, 92f);

            RectTransform sidesRow = CreateHorizontalGroup(transform, "SidesRow", 16f);
            SetPreferredHeight(sidesRow.gameObject, 220f);

            RectTransform partyPanel = CreatePanel(sidesRow, "PartyPanel");
            _partyText = CreateText(partyPanel, "PartyText", 22, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            _partyText.enableWordWrapping = true;
            _partyText.overflowMode = TextOverflowModes.Overflow;

            RectTransform enemyPanel = CreatePanel(sidesRow, "EnemyPanel");
            _enemyText = CreateText(enemyPanel, "EnemyText", 22, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            _enemyText.enableWordWrapping = true;
            _enemyText.overflowMode = TextOverflowModes.Overflow;

            RectTransform selectorPanel = CreatePanel(transform, "SelectorPanel");
            SetPreferredHeight(selectorPanel.gameObject, 300f);
            VerticalLayoutGroup selectorLayout = selectorPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            selectorLayout.padding = new RectOffset(14, 14, 14, 14);
            selectorLayout.spacing = 12f;
            selectorLayout.childControlWidth = true;
            selectorLayout.childControlHeight = false;
            selectorLayout.childForceExpandHeight = false;

            CreateSelectorRow(selectorPanel, "Actor", out _prevActorButton, out _actorText, out _nextActorButton);
            CreateSelectorRow(selectorPanel, "Action", out _prevActionButton, out _actionText, out _nextActionButton);
            CreateSelectorRow(selectorPanel, "Target", out _prevTargetButton, out _targetText, out _nextTargetButton);

            RectTransform controlsRow = CreateHorizontalGroup(transform, "ControlsRow", 12f);
            SetPreferredHeight(controlsRow.gameObject, 88f);
            _startDemoButton = CreateButton(controlsRow, "StartDemoButton", "Start Demo", 24);
            _applyActionButton = CreateButton(controlsRow, "ApplyActionButton", "Apply", 24);
            _endTurnButton = CreateButton(controlsRow, "EndTurnButton", "End Turn", 24);
            _closeButton = CreateButton(controlsRow, "CloseButton", "Close", 24);

            RectTransform logPanel = CreatePanel(transform, "LogPanel");
            VerticalLayoutGroup logLayout = logPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            logLayout.padding = new RectOffset(14, 14, 14, 14);
            logLayout.spacing = 8f;
            logLayout.childControlWidth = true;
            logLayout.childControlHeight = false;
            logLayout.childForceExpandHeight = false;
            SetPreferredHeight(logPanel.gameObject, 240f);

            TextMeshProUGUI logTitle = CreateText(logPanel, "LogTitle", 22, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            logTitle.text = "Combat Log";
            SetPreferredHeight(logTitle.gameObject, 32f);

            _logText = CreateText(logPanel, "LogText", 18, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            _logText.enableWordWrapping = true;
            _logText.overflowMode = TextOverflowModes.Truncate;
            SetPreferredHeight(_logText.gameObject, 176f);
        }

        private RectTransform CreatePanel(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.14f, 0.14f, 0.18f, 0.95f);
            return go.GetComponent<RectTransform>();
        }

        private RectTransform CreateHorizontalGroup(Transform parent, string name, float spacing)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            HorizontalLayoutGroup group = go.GetComponent<HorizontalLayoutGroup>();
            group.spacing = spacing;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = true;
            return go.GetComponent<RectTransform>();
        }

        private void CreateSelectorRow(
            Transform parent,
            string label,
            out Button prevButton,
            out TextMeshProUGUI valueText,
            out Button nextButton)
        {
            RectTransform row = CreateHorizontalGroup(parent, label + "Row", 10f);
            SetPreferredHeight(row.gameObject, 82f);

            prevButton = CreateButton(row, "Prev" + label + "Button", "<", 26);
            SetPreferredWidth(prevButton.gameObject, 72f);

            valueText = CreateText(row, label + "Text", 20, FontStyles.Normal, TextAlignmentOptions.Center);
            valueText.enableWordWrapping = false;
            valueText.overflowMode = TextOverflowModes.Ellipsis;
            valueText.text = label + ": —";

            nextButton = CreateButton(row, "Next" + label + "Button", ">", 26);
            SetPreferredWidth(nextButton.gameObject, 72f);
        }

        private Button CreateButton(Transform parent, string name, string text, float fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.color = new Color(0.24f, 0.24f, 0.3f, 1f);

            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.24f, 0.24f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.38f, 1f);
            colors.pressedColor = new Color(0.18f, 0.18f, 0.24f, 1f);
            colors.disabledColor = new Color(0.2f, 0.2f, 0.22f, 0.6f);
            button.colors = colors;

            TextMeshProUGUI label = CreateText(go.transform, "Label", fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
            label.text = text;

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        private TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            float fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSharedMaterial = TMP_Settings.defaultFontAsset != null
                ? TMP_Settings.defaultFontAsset.material
                : null;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = string.Empty;
            text.enableAutoSizing = false;
            text.raycastTarget = false;
            return text;
        }

        private static void SetPreferredHeight(GameObject gameObject, float height)
        {
            LayoutElement element = gameObject.GetComponent<LayoutElement>();
            if (element == null)
                element = gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
        }

        private static void SetPreferredWidth(GameObject gameObject, float width)
        {
            LayoutElement element = gameObject.GetComponent<LayoutElement>();
            if (element == null)
                element = gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = width;
        }

        private TextMeshProUGUI FindTmp(string childName)
        {
            Transform target = transform.Find(childName);
            if (target == null)
                target = FindDeepChild(transform, childName);
            return target != null ? target.GetComponent<TextMeshProUGUI>() : null;
        }

        private Button FindButton(string childName)
        {
            Transform target = transform.Find(childName);
            if (target == null)
                target = FindDeepChild(transform, childName);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                    return child;

                Transform nested = FindDeepChild(child, childName);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}
