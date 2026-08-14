using Modules.Windows.Scripts.Base;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.ListDialog
{
    /// <summary>
    /// Shared list/picker dialog shell. Content and item prefab come from <see cref="ListDialogViewModel"/>.
    /// </summary>
    public class ListDialogView : ViewBase<ListDialogViewModel>
    {
        public static string Path = "Prefabs/Views/Adventure/ListDialog/ListDialogView";

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Button _cancelButton;

        private readonly List<ListDialogItemViewBase> _spawnedItems = new List<ListDialogItemViewBase>();
        private ListDialogItemViewBase _itemPrefab;

        protected override void InitImplementation()
        {
            ApplyTitle();
            ApplyLayout();
            LoadItemPrefab();
            RebuildItems();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(_viewModel.OnCancel);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(_viewModel.OnCancel);

            ClearSpawned();
            _itemPrefab = null;
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == ListDialogViewModel.ON_CHANGE_ALL)
            {
                ApplyTitle();
                ApplyLayout();
                LoadItemPrefab();
                RebuildItems();
            }

            if (tag == ListDialogViewModel.ON_CHANGE_ITEMS)
            {
                RebuildItems();
            }
        }

        private void ApplyTitle()
        {
            if (_title != null)
                _title.text = _viewModel?.Title ?? string.Empty;
        }

        private void ApplyLayout()
        {
            if (_content == null || _viewModel?.Layout == null)
                return;

            ListDialogLayoutSettings settings = _viewModel.Layout;

            VerticalLayoutGroup vertical = _content.GetComponent<VerticalLayoutGroup>();
            GridLayoutGroup grid = _content.GetComponent<GridLayoutGroup>();

            if (settings.Mode == ListDialogLayoutMode.Grid)
            {
                if (vertical != null)
                    Object.DestroyImmediate(vertical);

                if (grid == null)
                    grid = _content.gameObject.AddComponent<GridLayoutGroup>();

                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = Mathf.Max(1, settings.ColumnCount);
                grid.cellSize = new Vector2(settings.CellWidth, settings.CellHeight);
                grid.spacing = new Vector2(settings.SpacingX, settings.SpacingY);
                grid.padding = settings.CreatePadding();
                grid.childAlignment = TextAnchor.UpperCenter;
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            }
            else
            {
                if (grid != null)
                    Object.DestroyImmediate(grid);

                if (vertical == null)
                    vertical = _content.gameObject.AddComponent<VerticalLayoutGroup>();

                vertical.spacing = settings.SpacingY;
                vertical.padding = settings.CreatePadding();
                vertical.childAlignment = TextAnchor.UpperCenter;
                vertical.childControlWidth = true;
                vertical.childControlHeight = true;
                vertical.childForceExpandWidth = true;
                vertical.childForceExpandHeight = false;
            }

            ContentSizeFitter fitter = _content.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = _content.gameObject.AddComponent<ContentSizeFitter>();

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void LoadItemPrefab()
        {
            _itemPrefab = null;

            string path = _viewModel?.ItemPrefabPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(ListDialogView)}] ItemPrefabPath is empty.",
                    this);
                return;
            }

            _itemPrefab = Resources.Load<ListDialogItemViewBase>(path);
            if (_itemPrefab == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(ListDialogView)}] Failed to load item prefab at '{path}'.",
                    this);
            }
        }

        private void RebuildItems()
        {
            ClearSpawned();

            if (_content == null || _itemPrefab == null || _viewModel?.Items == null)
                return;

            ListDialogLayoutSettings settings = _viewModel.Layout;
            IReadOnlyList<ListDialogItemViewModelBase> items = _viewModel.Items;

            for (int i = 0; i < items.Count; i++)
            {
                ListDialogItemViewModelBase itemVm = items[i];
                if (itemVm == null || itemVm.IsDisposed)
                    continue;

                ListDialogItemViewBase instance = Object.Instantiate(_itemPrefab, _content);
                instance.gameObject.SetActive(true);
                instance.name = "Item_" + (string.IsNullOrEmpty(itemVm.Id) ? i.ToString() : itemVm.Id);

                if (settings != null
                    && settings.Mode == ListDialogLayoutMode.Vertical
                    && settings.CellHeight > 0f)
                {
                    LayoutElement layoutElement = instance.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                        layoutElement = instance.gameObject.AddComponent<LayoutElement>();

                    layoutElement.preferredHeight = settings.CellHeight;
                    layoutElement.minHeight = settings.CellHeight;
                }

                instance.Init(itemVm);
                _spawnedItems.Add(instance);
            }
        }

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                ListDialogItemViewBase item = _spawnedItems[i];
                if (item == null)
                    continue;

                Object.Destroy(item.gameObject);
            }

            _spawnedItems.Clear();
        }
    }
}
