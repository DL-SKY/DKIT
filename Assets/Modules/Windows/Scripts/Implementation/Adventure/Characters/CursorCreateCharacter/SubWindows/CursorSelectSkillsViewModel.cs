using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public sealed class CursorSkillRowData
    {
        public string Key;
        public string Title;
        public string IconPath;
        public int Rank;
        public string RankLabel;
    }

    /// <summary>
    /// Lets the player pick Pathfinder proficiency ranks for skills on the draft character.
    /// </summary>
    public class CursorSelectSkillsViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ALL = "ON_CHANGE_ALL";

        private static readonly string[] RankLabels =
        {
            "Untrained",
            "Trained",
            "Expert",
            "Master",
            "Legendary",
        };

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private bool _isDisposed;
        private CreateCharacterRequestData _request;
        private Action _onChanged;
        private readonly List<CursorSkillRowData> _rows = new List<CursorSkillRowData>();

        public IReadOnlyList<CursorSkillRowData> Rows => _rows;

        public void Init(CreateCharacterRequestData request, Action onChanged)
        {
            _isDisposed = false;
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _onChanged = onChanged;
            RebuildRows(notify: false);
        }

        public void OnCycleRank(string skillKey)
        {
            if (_isDisposed || string.IsNullOrEmpty(skillKey))
                return;

            int current = CursorCreateCharacterDraftApplier.GetSkillProficiencyRank(_request, skillKey);
            int next = current + 1;
            if (next > (int)ProficiencyType.Legendary)
                next = (int)ProficiencyType.Untrained;

            CursorCreateCharacterDraftApplier.SetSkillProficiencyRank(_request, skillKey, next);
            _onChanged?.Invoke();
            RebuildRows(notify: true);
        }

        public void OnClose()
        {
            if (_isDisposed)
                return;

            Close();
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _request = null;
            _onChanged = null;
            _rows.Clear();
        }

        protected override Options CreateOptions()
        {
            return new Options(true, false, SortingOrderLayer.DIALOGUE);
        }

        private void RebuildRows(bool notify)
        {
            _rows.Clear();

            string[] keys = CursorCreateCharacterDraftApplier.SkillKeys;
            var icons = _definitionsManager?.VisualSettings?.ParameterIcons;
            var locs = _definitionsManager?.VisualSettings?.ParameterLocalizations;

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                string title = key;
                if (locs != null && locs.TryGetValue(key, out string loc) && !string.IsNullOrWhiteSpace(loc))
                    title = loc;

                string iconPath = string.Empty;
                if (icons != null && icons.TryGetValue(key, out string path) && path != null)
                    iconPath = path;

                int rank = CursorCreateCharacterDraftApplier.GetSkillProficiencyRank(_request, key);
                if (rank < 0)
                    rank = 0;
                if (rank >= RankLabels.Length)
                    rank = RankLabels.Length - 1;

                _rows.Add(new CursorSkillRowData
                {
                    Key = key,
                    Title = title,
                    IconPath = iconPath,
                    Rank = rank,
                    RankLabel = RankLabels[rank],
                });
            }

            if (notify)
                SendOnChange(ON_CHANGE_ALL);
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }
    }
}
