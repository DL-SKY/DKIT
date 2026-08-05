using Modules.Windows.Scripts.Base;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public class CursorSelectAvatarView : ViewBase<CursorSelectAvatarViewModel>
    {
        public static string Path =
            "Prefabs/Views/Adventure/Characters/CursorCreateCharacter/CursorSelectAvatarView";

        [SerializeField] private Transform _content;
        [SerializeField] private Button _itemTemplate;
        [SerializeField] private Button _cancelButton;

        private readonly List<Button> _spawnedButtons = new List<Button>();

        protected override void InitImplementation()
        {
            if (_itemTemplate != null)
                _itemTemplate.gameObject.SetActive(false);

            RebuildList();
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
        }

        private void OnChangeCustomHandler(string tag)
        {
            if (tag == CursorSelectAvatarViewModel.ON_CHANGE_ITEMS)
                RebuildList();
        }

        private void RebuildList()
        {
            ClearSpawned();

            if (_content == null || _itemTemplate == null || _viewModel?.Items == null)
                return;

            IReadOnlyList<CursorAvatarListItem> items = _viewModel.Items;
            for (int i = 0; i < items.Count; i++)
            {
                CursorAvatarListItem item = items[i];
                Button button = Instantiate(_itemTemplate, _content);
                button.gameObject.SetActive(true);

                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    string mark = item.IsSelected ? "✓ " : string.Empty;
                    label.text = mark + item.Title;
                }

                string path = item.Path;
                button.onClick.AddListener(() => _viewModel.OnSelect(path));
                _spawnedButtons.Add(button);
            }
        }

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawnedButtons.Count; i++)
            {
                Button button = _spawnedButtons[i];
                if (button == null)
                    continue;

                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }

            _spawnedButtons.Clear();
        }
    }
}
