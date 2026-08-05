using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows
{
    public sealed class CursorAbilityRowData
    {
        public string Key;
        public string Title;
        public string IconPath;
        public int Value;
    }

    /// <summary>
    /// Distributes free BoostPoints across STR/DEX/CON/INT/WIS/CHA on the draft character.
    /// </summary>
    public class CursorDistributeAbilitiesViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ALL = "ON_CHANGE_ALL";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly DefinitionsManager _definitionsManager;

        private bool _isDisposed;
        private CreateCharacterRequestData _request;
        private Action _onChanged;
        private readonly Dictionary<string, int> _spentByAbility = new Dictionary<string, int>();
        private readonly List<CursorAbilityRowData> _rows = new List<CursorAbilityRowData>();

        public int RemainingBoostPoints { get; private set; }
        public IReadOnlyList<CursorAbilityRowData> Rows => _rows;

        public void Init(CreateCharacterRequestData request, Action onChanged)
        {
            _isDisposed = false;
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _onChanged = onChanged;
            _spentByAbility.Clear();

            IReadOnlyList<string> keys = CursorCreateCharacterDraftApplier.AbilityKeysReadonly;
            for (int i = 0; i < keys.Count; i++)
                _spentByAbility[keys[i]] = 0;

            RebuildRows(notify: false);
        }

        public void OnIncrease(string abilityKey)
        {
            if (_isDisposed || string.IsNullOrEmpty(abilityKey))
                return;

            int remaining = CursorCreateCharacterDraftApplier.GetBoostPoints(_request);
            if (remaining <= 0)
                return;

            int current = CursorCreateCharacterDraftApplier.GetRawParameter(_request, abilityKey);
            CursorCreateCharacterDraftApplier.SetRawParameter(_request, abilityKey, current + 1);
            CursorCreateCharacterDraftApplier.SetRawParameter(
                _request,
                Glossary.Characters.BOOST_POINTS,
                remaining - 1);

            if (_spentByAbility.ContainsKey(abilityKey))
                _spentByAbility[abilityKey]++;

            _onChanged?.Invoke();
            RebuildRows(notify: true);
        }

        public void OnDecrease(string abilityKey)
        {
            if (_isDisposed || string.IsNullOrEmpty(abilityKey))
                return;

            if (!_spentByAbility.TryGetValue(abilityKey, out int spent) || spent <= 0)
                return;

            int current = CursorCreateCharacterDraftApplier.GetRawParameter(_request, abilityKey);
            CursorCreateCharacterDraftApplier.SetRawParameter(_request, abilityKey, current - 1);

            int remaining = CursorCreateCharacterDraftApplier.GetBoostPoints(_request);
            CursorCreateCharacterDraftApplier.SetRawParameter(
                _request,
                Glossary.Characters.BOOST_POINTS,
                remaining + 1);

            _spentByAbility[abilityKey] = spent - 1;
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
            _spentByAbility.Clear();
            _rows.Clear();
        }

        protected override Options CreateOptions()
        {
            return new Options(true, false, SortingOrderLayer.DIALOGUE);
        }

        private void RebuildRows(bool notify)
        {
            _rows.Clear();
            RemainingBoostPoints = CursorCreateCharacterDraftApplier.GetBoostPoints(_request);

            IReadOnlyList<string> keys = CursorCreateCharacterDraftApplier.AbilityKeysReadonly;
            var icons = _definitionsManager?.VisualSettings?.ParameterIcons;
            var locs = _definitionsManager?.VisualSettings?.ParameterLocalizations;

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                string title = key;
                if (locs != null && locs.TryGetValue(key, out string loc) && !string.IsNullOrWhiteSpace(loc))
                    title = loc;

                string iconPath = string.Empty;
                if (icons != null && icons.TryGetValue(key, out string path) && path != null)
                    iconPath = path;

                _rows.Add(new CursorAbilityRowData
                {
                    Key = key,
                    Title = title,
                    IconPath = iconPath,
                    Value = CursorCreateCharacterDraftApplier.GetRawParameter(_request, key),
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
