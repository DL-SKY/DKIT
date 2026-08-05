using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public class CursorSelectSkillsView : ViewBase<CursorSelectSkillsViewModel>
    {
        public static string Path =
            "Prefabs/Views/Adventure/Characters/CursorCreateCharacter/CursorSelectSkillsView";

        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _rowTemplate;
        [SerializeField] private Button _closeButton;

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();

        protected override void InitImplementation()
        {
            if (_rowTemplate != null)
                _rowTemplate.SetActive(false);

            RebuildRows();
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
            if (tag == CursorSelectSkillsViewModel.ON_CHANGE_ALL)
                RebuildRows();
        }

        private void RebuildRows()
        {
            ClearSpawned();

            if (_content == null || _rowTemplate == null || _viewModel?.Rows == null)
                return;

            IReadOnlyList<CursorSkillRowData> rows = _viewModel.Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                CursorSkillRowData row = rows[i];
                GameObject instance = Instantiate(_rowTemplate, _content);
                instance.SetActive(true);
                instance.name = "Skill_" + row.Key;

                CachedPathImage icon = instance.GetComponentInChildren<CachedPathImage>(true);
                if (icon != null)
                    icon.SetPath(row.IconPath);

                Transform titleTransform = instance.transform.Find("Title");
                if (titleTransform != null)
                {
                    TextMeshProUGUI title = titleTransform.GetComponent<TextMeshProUGUI>();
                    if (title != null)
                        title.text = row.Title;
                }

                Transform rankTransform = instance.transform.Find("Rank");
                if (rankTransform != null)
                {
                    TextMeshProUGUI rank = rankTransform.GetComponent<TextMeshProUGUI>();
                    if (rank != null)
                        rank.text = row.RankLabel;
                }

                Button button = instance.GetComponent<Button>();
                string key = row.Key;
                if (button != null)
                    button.onClick.AddListener(() => _viewModel.OnCycleRank(key));

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

                Button button = go.GetComponentInChildren<Button>(true);
                if (button != null)
                    button.onClick.RemoveAllListeners();

                Destroy(go);
            }

            _spawnedRows.Clear();
        }
    }
}
