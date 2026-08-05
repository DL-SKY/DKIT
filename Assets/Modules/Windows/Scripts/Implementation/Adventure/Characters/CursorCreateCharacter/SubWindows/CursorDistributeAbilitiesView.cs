using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public class CursorDistributeAbilitiesView : ViewBase<CursorDistributeAbilitiesViewModel>
    {
        public static string Path =
            "Prefabs/Views/Adventure/Characters/CursorCreateCharacter/CursorDistributeAbilitiesView";

        [SerializeField] private TextMeshProUGUI _boostPointsLabel;
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _rowTemplate;
        [SerializeField] private Button _closeButton;

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();

        protected override void InitImplementation()
        {
            if (_rowTemplate != null)
                _rowTemplate.SetActive(false);

            ApplyAll();
        }

        protected override void Subscribe()
        {
            _viewModel.OnChangeCustom += OnChangeCustomHandler;

            if (_closeButton != null)
                _closeButton.onClick.AddListener(_viewModel.OnClose);
        }

        protected override void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.OnChangeCustom -= OnChangeCustomHandler;

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(_viewModel.OnClose);

            ClearSpawned();
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == CursorDistributeAbilitiesViewModel.ON_CHANGE_ALL)
                ApplyAll();
        }

        private void ApplyAll()
        {
            if (_boostPointsLabel != null)
                _boostPointsLabel.text = $"Свободные очки: {_viewModel.RemainingBoostPoints}";

            RebuildRows();
        }

        private void RebuildRows()
        {
            ClearSpawned();

            if (_content == null || _rowTemplate == null || _viewModel?.Rows == null)
                return;

            IReadOnlyList<CursorAbilityRowData> rows = _viewModel.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                CursorAbilityRowData row = rows[i];
                GameObject instance = Instantiate(_rowTemplate, _content);
                instance.SetActive(true);
                instance.name = "Ability_" + row.Key;

                CachedPathImage icon = instance.GetComponentInChildren<CachedPathImage>(true);
                if (icon != null)
                    icon.SetPath(row.IconPath);

                TextMeshProUGUI title = FindTmp(instance.transform, "Title");
                if (title != null)
                    title.text = row.Title;

                TextMeshProUGUI value = FindTmp(instance.transform, "Value");
                if (value != null)
                    value.text = FormatSigned(row.Value);

                string key = row.Key;
                Button decrease = FindButton(instance.transform, "DecreaseButton");
                if (decrease != null)
                    decrease.onClick.AddListener(() => _viewModel.OnDecrease(key));

                Button increase = FindButton(instance.transform, "IncreaseButton");
                if (increase != null)
                    increase.onClick.AddListener(() => _viewModel.OnIncrease(key));

                _spawnedRows.Add(instance);
            }
        }

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawnedRows.Count; i++)
            {
                GameObject go = _spawnedRows[i];
                if (go == null)
                    continue;

                Button[] buttons = go.GetComponentsInChildren<Button>(true);
                if (buttons != null)
                {
                    for (int b = 0; b < buttons.Length; b++)
                        buttons[b].onClick.RemoveAllListeners();
                }

                Destroy(go);
            }

            _spawnedRows.Clear();
        }

        private static string FormatSigned(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }

        private static TextMeshProUGUI FindTmp(Transform root, string childName)
        {
            Transform child = root != null ? root.Find(childName) : null;
            return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }

        private static Button FindButton(Transform root, string childName)
        {
            Transform child = root != null ? root.Find(childName) : null;
            return child != null ? child.GetComponent<Button>() : null;
        }
    }
}
