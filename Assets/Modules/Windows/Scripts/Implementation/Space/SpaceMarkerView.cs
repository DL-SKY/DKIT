using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Button that follows one sector target. A missing type sprite keeps the white image on the prefab.
    /// The radial buttons are exactly the view-model list. This view does not choose which commands appear.
    /// </summary>
    public sealed class SpaceMarkerView : MonoBehaviour
    {
        public const string Path = "Prefabs/Views/Space/SpaceMarkerView";

        private const float MENU_BUTTON_SIZE = 44f;
        private float _appliedMenuRadius;
        private const float EDGE_MARGIN = 36f;

        private Button _button;
        private Image _icon;
        private TextMeshProUGUI _speedLabel;
        private SpaceMarkerViewModel _viewModel;
        private readonly List<GameObject> _menuButtons = new List<GameObject>();

        public string TargetId => _viewModel != null ? _viewModel.TargetId : null;

        public void Init(SpaceMarkerViewModel viewModel)
        {
            Unsubscribe();
            _viewModel = viewModel;
            if (_button == null)
                _button = GetComponent<Button>();
            if (_icon == null)
                _icon = GetComponent<Image>();

            ApplyIcon();
            EnsureSpeedLabel();
            ApplySpeed();
            ApplyMenu();
            Subscribe();
        }

        public void Place(RectTransform holder)
        {
            if (_viewModel == null || holder == null)
                return;

            var camera = Camera.main;
            if (camera == null)
                return;

            if (!SpaceMarkerScreen.TryLocalPoint(holder, camera, _viewModel.WorldPosition, EDGE_MARGIN, out var local))
                return;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = local;
            ApplySpeed();
            if (_viewModel.MenuOpen && !Mathf.Approximately(_appliedMenuRadius, _viewModel.MenuRadius))
                ApplyMenu();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            ClearMenu();
        }

        private void Subscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom += OnMenuChanged;

            if (_button != null)
                _button.onClick.AddListener(OnButtonClick);
        }

        private void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnMenuChanged;

            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnMenuChanged(string tag)
        {
            if (tag == SpaceMarkerViewModel.ON_MENU)
                ApplyMenu();
        }

        private void OnButtonClick()
        {
            _viewModel?.Click();
        }

        private void EnsureSpeedLabel()
        {
            if (_viewModel == null || !_viewModel.ShowSpeed || _speedLabel != null)
                return;

            var labelObject = new GameObject("Speed", typeof(RectTransform));
            labelObject.transform.SetParent(transform, false);
            _speedLabel = labelObject.AddComponent<TextMeshProUGUI>();
            _speedLabel.fontSize = 14f;
            _speedLabel.alignment = TextAlignmentOptions.Center;
            _speedLabel.color = Color.white;
            _speedLabel.raycastTarget = false;
            _speedLabel.enableWordWrapping = false;
            if (TMP_Settings.defaultFontAsset != null)
                _speedLabel.font = TMP_Settings.defaultFontAsset;

            var rect = _speedLabel.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(140f, 18f);
            rect.anchoredPosition = new Vector2(0f, -26f);
        }

        private void ApplySpeed()
        {
            if (_speedLabel == null || _viewModel == null)
                return;

            var max = _viewModel.MaxMetersPerSecond;
            var percent = max > 0f
                ? Mathf.RoundToInt(_viewModel.MetersPerSecond / max * 100f)
                : 0;
            _speedLabel.text = _viewModel.MetersPerSecond.ToString("0.00") + " (" + percent + "%)";
        }

        private void ApplyIcon()
        {
            if (_icon == null || _viewModel == null || string.IsNullOrEmpty(_viewModel.IconResourcesPath))
                return;

            var sprite = Resources.Load<Sprite>(_viewModel.IconResourcesPath);
            if (sprite != null)
                _icon.sprite = sprite;
        }

        private void ApplyMenu()
        {
            ClearMenu();
            if (_viewModel == null || !_viewModel.MenuOpen)
                return;
            var buttons = _viewModel.MenuButtons;
            var count = buttons.Count;
            if (count == 0)
                return;

            var step = 360f / count;
            for (var i = 0; i < count; i++)
            {
                var menuButton = buttons[i];
                var angle = (90f - step * i) * Mathf.Deg2Rad;
                var button = CreateMenuButton(menuButton);
                var rect = (RectTransform)button.transform;
                rect.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _viewModel.MenuRadius;
                _menuButtons.Add(button);
            }

            _appliedMenuRadius = _viewModel.MenuRadius;
        }

        private GameObject CreateMenuButton(SpaceMarkerMenuButton menuButton)
        {
            var buttonObject = new GameObject(menuButton.CommandId, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(transform, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(MENU_BUTTON_SIZE, MENU_BUTTON_SIZE);
            var image = buttonObject.GetComponent<Image>();
            var sprite = string.IsNullOrEmpty(menuButton.IconResourcesPath)
                ? null
                : Resources.Load<Sprite>(menuButton.IconResourcesPath);
            image.sprite = sprite;
            image.color = Color.white;
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => _viewModel?.ClickCommand(menuButton.CommandId, menuButton.SourceId));

            var labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = menuButton.Label;
            label.fontSize = 12f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.1f, 0.16f, 0.22f, 1f);
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            return buttonObject;
        }

        private void ClearMenu()
        {
            for (var i = 0; i < _menuButtons.Count; i++)
            {
                if (_menuButtons[i] != null)
                    Destroy(_menuButtons[i]);
            }

            _menuButtons.Clear();
            _appliedMenuRadius = 0f;
        }
    }
}
