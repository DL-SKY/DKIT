using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.Actions;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Logic;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter.SubWindows;
using Modules.Windows.Scripts.Implementation.Adventure.ListDialog;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System.Collections.Generic;
using Zenject;
using Zenject.Scripts.Factories;

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter
{
    /// <summary>
    /// Cursor duplicate of character creation flow. Does not modify CreateCharacterViewModel.
    /// </summary>
    public class CursorCreateCharacterViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ALL = "ON_CHANGE_ALL";
        public const string ON_CHANGE_CAN_CREATE = "ON_CHANGE_CAN_CREATE";

        [Inject] private readonly WindowsManager _windowsManager;
        [Inject] private readonly AdventureStateManager _stateManager;
        [Inject] private readonly AdventureStateLogic _stateLogic;
        [Inject] private readonly DefinitionsManager _definitionsManager;
        [Inject] private readonly ViewModelFactory _viewModelFactory;

        private bool _isInitialized;
        private bool _isDisposed;
        private CreateCharacterRequestData _request;

        public bool CanCreate { get; private set; }

        public string AvatarPath { get; private set; } = string.Empty;
        public string NameText { get; private set; } = string.Empty;
        public string AncestryText { get; private set; } = string.Empty;
        public string ClassText { get; private set; } = string.Empty;
        public string BackgroundText { get; private set; } = string.Empty;
        public int BoostPoints { get; private set; }

        public IReadOnlyList<string> AbilityKeys => CursorCreateCharacterDraftApplier.AbilityKeysReadonly;

        public void Init()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _isDisposed = false;

            _request = new CreateCharacterRequestData
            {
                AddToActiveParty = true,
                Gender = CharacterGender.Male,
                CharacterData = new CharacterRequestData(),
            };

            string unknownAvatar = _definitionsManager?.VisualSettings?.UnknownCharacterAvatar;
            if (!string.IsNullOrWhiteSpace(unknownAvatar))
            {
                _request.Avatar = unknownAvatar;
                AvatarPath = unknownAvatar;
            }

            CursorCreateCharacterDraftApplier.RebuildFromSelections(_request, _definitionsManager);
            RefreshPresentation(notify: false);
            RefreshCanCreate();
        }

        public int GetAbilityValue(string abilityKey)
        {
            return CursorCreateCharacterDraftApplier.GetRawParameter(_request, abilityKey);
        }

        public string GetAbilityIconPath(string abilityKey)
        {
            var icons = _definitionsManager?.VisualSettings?.ParameterIcons;
            if (icons == null || string.IsNullOrEmpty(abilityKey))
                return string.Empty;

            return icons.TryGetValue(abilityKey, out string path) && path != null
                ? path
                : string.Empty;
        }

        public void OnClose()
        {
            if (_isDisposed)
                return;

            Close();
        }

        public void OnCreateActive()
        {
            if (_isDisposed || !CanCreate || _request == null)
                return;

            int newCharacterId = _stateManager.State?.Characters?.NextCharacterId ?? 0;

            StateActionValidationResult createResult =
                _stateLogic.ProcessAction(new CreateCharacterStateAction(_request));
            if (!createResult.IsValid)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CursorCreateCharacterViewModel)}] Create failed " +
                    $"({createResult.ErrorCode}): {createResult.ErrorMessage}");
                return;
            }

            StateActionValidationResult setActiveResult =
                _stateLogic.ProcessAction(new SetCurrentActiveCharacterIdStateAction(newCharacterId));
            if (!setActiveResult.IsValid)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CursorCreateCharacterViewModel)}] Set active id={newCharacterId} failed " +
                    $"({setActiveResult.ErrorCode}): {setActiveResult.ErrorMessage}");
            }

            Close();
        }

        public void OnCreateDisable()
        {
            // Intentional no-op; Create button is disabled until required fields are filled.
        }

        public void OnEditAvatar()
        {
            if (_isDisposed)
                return;

            var vm = _viewModelFactory.Create<AvatarListDialogViewModel>();
            vm.Init(_request.Avatar, OnAvatarSelected);
            _windowsManager.OpenView<ListDialogView, ListDialogViewModel>(
                ListDialogView.Path, vm);
        }

        public void OnEditName()
        {
            if (_isDisposed)
                return;

            var vm = _viewModelFactory.Create<CursorEditNameViewModel>();
            vm.Init(_request.Name, OnNameSelected);
            _windowsManager.OpenView<CursorEditNameView, CursorEditNameViewModel>(
                CursorEditNameView.Path, vm);
        }

        public void OnEditAncestry()
        {
            if (_isDisposed)
                return;

            OpenDefinitionPicker(
                title: "Происхождение",
                mode: DefinitionListDialogMode.Ancestry,
                selectedId: _request.Ancestry,
                onSelected: OnAncestrySelected);
        }

        public void OnEditClass()
        {
            if (_isDisposed)
                return;

            OpenDefinitionPicker(
                title: "Класс",
                mode: DefinitionListDialogMode.Class,
                selectedId: _request.Class,
                onSelected: OnClassSelected);
        }

        public void OnEditBackground()
        {
            if (_isDisposed)
                return;

            OpenDefinitionPicker(
                title: "Предыстория",
                mode: DefinitionListDialogMode.Background,
                selectedId: _request.Background,
                onSelected: OnBackgroundSelected);
        }

        public void OnEditAbilities()
        {
            if (_isDisposed)
                return;

            var vm = _viewModelFactory.Create<CursorDistributeAbilitiesViewModel>();
            vm.Init(_request, OnAbilitiesChanged);
            _windowsManager.OpenView<CursorDistributeAbilitiesView, CursorDistributeAbilitiesViewModel>(
                CursorDistributeAbilitiesView.Path, vm);
        }

        public void OnEditSkills()
        {
            if (_isDisposed)
                return;

            var vm = _viewModelFactory.Create<CursorSelectSkillsViewModel>();
            vm.Init(_request, OnSkillsChanged);
            _windowsManager.OpenView<CursorSelectSkillsView, CursorSelectSkillsViewModel>(
                CursorSelectSkillsView.Path, vm);
        }

        public override void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isInitialized = false;
            _request = null;
            CanCreate = false;
        }

        protected override Options CreateOptions()
        {
            return new Options(
                canCloseOnEsc: true,
                hideInHistory: false,
                sortingLayer: SortingOrderLayer.COMMON);
        }

        private void OpenDefinitionPicker(
            string title,
            DefinitionListDialogMode mode,
            string selectedId,
            System.Action<string> onSelected)
        {
            var vm = _viewModelFactory.Create<DefinitionListDialogViewModel>();
            vm.Init(title, mode, selectedId, onSelected);
            _windowsManager.OpenView<ListDialogView, ListDialogViewModel>(
                ListDialogView.Path, vm);
        }

        private void OnAvatarSelected(string avatarPath)
        {
            if (_isDisposed || _request == null)
                return;

            _request.Avatar = avatarPath ?? string.Empty;
            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void OnNameSelected(string name)
        {
            if (_isDisposed || _request == null)
                return;

            _request.Name = name ?? string.Empty;
            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void OnAncestrySelected(string ancestryId)
        {
            if (_isDisposed || _request == null)
                return;

            _request.Ancestry = ancestryId ?? string.Empty;
            CursorCreateCharacterDraftApplier.RebuildFromSelections(_request, _definitionsManager);
            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void OnClassSelected(string classId)
        {
            if (_isDisposed || _request == null)
                return;

            _request.Class = classId ?? string.Empty;
            CursorCreateCharacterDraftApplier.RebuildFromSelections(_request, _definitionsManager);
            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void OnBackgroundSelected(string backgroundId)
        {
            if (_isDisposed || _request == null)
                return;

            _request.Background = backgroundId ?? string.Empty;
            CursorCreateCharacterDraftApplier.RebuildFromSelections(_request, _definitionsManager);
            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void OnAbilitiesChanged()
        {
            if (_isDisposed)
                return;

            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void OnSkillsChanged()
        {
            if (_isDisposed)
                return;

            RefreshPresentation(notify: true);
            RefreshCanCreate();
        }

        private void RefreshPresentation(bool notify)
        {
            AvatarPath = _request?.Avatar ?? string.Empty;
            NameText = string.IsNullOrWhiteSpace(_request?.Name) ? "—" : _request.Name;
            AncestryText = ResolveAncestryTitle(_request?.Ancestry);
            ClassText = ResolveClassTitle(_request?.Class);
            BackgroundText = ResolveBackgroundTitle(_request?.Background);
            BoostPoints = CursorCreateCharacterDraftApplier.GetBoostPoints(_request);

            if (notify)
                SendOnChange(ON_CHANGE_ALL);
        }

        private void RefreshCanCreate()
        {
            bool canCreate = _request != null
                && !string.IsNullOrWhiteSpace(_request.Name)
                && !string.IsNullOrWhiteSpace(_request.Ancestry)
                && !string.IsNullOrWhiteSpace(_request.Class);

            if (CanCreate == canCreate)
                return;

            CanCreate = canCreate;
            SendOnChange(ON_CHANGE_CAN_CREATE);
        }

        private string ResolveAncestryTitle(string id)
        {
            if (string.IsNullOrWhiteSpace(id)
                || _definitionsManager?.Ancestries == null
                || !_definitionsManager.Ancestries.TryGetValue(id, out AncestryDef def)
                || def == null)
            {
                return "—";
            }

            return string.IsNullOrWhiteSpace(def.Title) ? id : def.Title;
        }

        private string ResolveClassTitle(string id)
        {
            if (string.IsNullOrWhiteSpace(id)
                || _definitionsManager?.Classes == null
                || !_definitionsManager.Classes.TryGetValue(id, out ClassDef def)
                || def == null)
            {
                return "—";
            }

            return string.IsNullOrWhiteSpace(def.Title) ? id : def.Title;
        }

        private string ResolveBackgroundTitle(string id)
        {
            if (string.IsNullOrWhiteSpace(id)
                || _definitionsManager?.Backgrounds == null
                || !_definitionsManager.Backgrounds.TryGetValue(id, out BackgroundDef def)
                || def == null)
            {
                return "—";
            }

            return string.IsNullOrWhiteSpace(def.Title) ? id : def.Title;
        }

        private void Close()
        {
            _windowsManager.CloseView(ViewHandle);
        }
    }
}
