using Modules.Definitions.Scripts.Implementation.Adventures.Constants;
using Modules.RPG.Scripts.Adventure.CursorBattlePrototype;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Managers;
using Modules.Windows.Scripts.Settings;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.CursorBattlePrototype
{
    /// <summary>
    /// ViewModel for prototype combat window with mobile-friendly controls.
    /// </summary>
    public sealed class CursorBattlePrototypeViewModel : ViewModelBase
    {
        public const string ON_CHANGE_ALL = "ON_CHANGE_ALL";

        [Inject] private readonly DiContainer _container;
        [Inject] private readonly WindowsManager _windowsManager;
        private CursorBattlePrototypeService _service;
        private int _cachedLogCount;

        private readonly List<CursorBattlePrototypeActor> _actorOptions = new List<CursorBattlePrototypeActor>();
        private readonly List<CursorBattlePrototypeActionPreview> _actionOptions = new List<CursorBattlePrototypeActionPreview>();
        private readonly List<CursorBattlePrototypeActor> _targetOptions = new List<CursorBattlePrototypeActor>();

        private int _actorIndex;
        private int _actionIndex;
        private int _targetIndex;

        public bool HasSession { get; private set; }
        public bool IsPlayerTurn { get; private set; }
        public string HeaderText { get; private set; } = string.Empty;
        public string PartySummaryText { get; private set; } = string.Empty;
        public string EnemySummaryText { get; private set; } = string.Empty;
        public string ActorSelectorText { get; private set; } = "—";
        public string ActionSelectorText { get; private set; } = "—";
        public string TargetSelectorText { get; private set; } = "—";
        public string LogText { get; private set; } = string.Empty;

        public void Init()
        {
            _service = _container.Instantiate<CursorBattlePrototypeService>();
            Refresh(forceNotify: false);
        }

        public void Tick()
        {
            if (_service?.Session != null && _service.Session.IsActive && !_service.IsPlayerTurn())
                _service.RunAiIfNeeded();

            bool shouldRefresh = ShouldRefreshFromSession();
            if (shouldRefresh)
                Refresh(forceNotify: true);
        }

        public void OnStartDemoEncounter()
        {
            if (_service == null)
                return;

            _service.StartDemoEncounter();
            ResetSelectors();
            Refresh(forceNotify: true);
        }

        public void StartDemoEncounterIfNeeded()
        {
            if (_service == null)
                return;

            if (_service.Session != null && _service.Session.IsActive)
                return;

            OnStartDemoEncounter();
        }

        public void OnApplyAction()
        {
            if (_service == null || !HasSession || !IsPlayerTurn)
                return;

            if (_actorOptions.Count == 0 || _actionOptions.Count == 0)
                return;

            int actorRuntimeId = _actorOptions[_actorIndex].RuntimeId;
            string actionId = _actionOptions[_actionIndex].ActionId;
            int targetRuntimeId = _targetOptions.Count > 0 ? _targetOptions[_targetIndex].RuntimeId : 0;

            _service.TryApplyAction(actorRuntimeId, actionId, targetRuntimeId);
            Refresh(forceNotify: true);
        }

        public void OnEndTurn()
        {
            if (_service == null || !HasSession)
                return;

            _service.EndTurn();
            Refresh(forceNotify: true);
        }

        public void OnPrevActor()
        {
            if (_actorOptions.Count == 0)
                return;

            _actorIndex = WrapIndex(_actorIndex - 1, _actorOptions.Count);
            RebuildActionAndTargetOptions();
            Refresh(forceNotify: true);
        }

        public void OnNextActor()
        {
            if (_actorOptions.Count == 0)
                return;

            _actorIndex = WrapIndex(_actorIndex + 1, _actorOptions.Count);
            RebuildActionAndTargetOptions();
            Refresh(forceNotify: true);
        }

        public void OnPrevAction()
        {
            if (_actionOptions.Count == 0)
                return;

            _actionIndex = WrapIndex(_actionIndex - 1, _actionOptions.Count);
            RebuildTargetOptions();
            Refresh(forceNotify: true);
        }

        public void OnNextAction()
        {
            if (_actionOptions.Count == 0)
                return;

            _actionIndex = WrapIndex(_actionIndex + 1, _actionOptions.Count);
            RebuildTargetOptions();
            Refresh(forceNotify: true);
        }

        public void OnPrevTarget()
        {
            if (_targetOptions.Count == 0)
                return;

            _targetIndex = WrapIndex(_targetIndex - 1, _targetOptions.Count);
            Refresh(forceNotify: true);
        }

        public void OnNextTarget()
        {
            if (_targetOptions.Count == 0)
                return;

            _targetIndex = WrapIndex(_targetIndex + 1, _targetOptions.Count);
            Refresh(forceNotify: true);
        }

        public void OnClose()
        {
            _windowsManager.CloseView(ViewHandle);
        }

        public override void Dispose()
        {
            _service = null;
            _actorOptions.Clear();
            _actionOptions.Clear();
            _targetOptions.Clear();
        }

        protected override Options CreateOptions()
        {
            return new Options(canCloseOnEsc: true, hideInHistory: false, sortingLayer: SortingOrderLayer.COMMON);
        }

        private bool ShouldRefreshFromSession()
        {
            if (_service == null)
                return false;

            CursorBattlePrototypeSession session = _service.Session;
            if (session == null)
                return HasSession;

            if (HasSession != session.IsActive)
                return true;

            if (session.Log != null && session.Log.Count != _cachedLogCount)
                return true;

            return false;
        }

        private void Refresh(bool forceNotify)
        {
            if (_service == null || _service.Session == null)
            {
                ApplyNoSessionState();
                if (forceNotify)
                    SendOnChange(ON_CHANGE_ALL);
                return;
            }

            CursorBattlePrototypeSession session = _service.Session;
            HasSession = session.IsActive;
            IsPlayerTurn = _service.IsPlayerTurn();
            _cachedLogCount = session.Log != null ? session.Log.Count : 0;

            HeaderText = $"Round: {session.Round}  |  Side: {_service.GetCurrentSideId()}  |  AP: {session.RemainingActions}/{session.ActionsPerTurn}";
            PartySummaryText = BuildSideSummary(session, 0, "Party");
            EnemySummaryText = BuildSideSummary(session, 1, "Enemies");

            RebuildActorOptions();
            RebuildActionAndTargetOptions();
            UpdateSelectorTexts();
            LogText = BuildCompactLog(session);

            if (!session.IsActive && session.ResultType != CursorBattlePrototypeResultType.None)
                HeaderText = $"{HeaderText}\nResult: {session.ResultType}";

            if (forceNotify)
                SendOnChange(ON_CHANGE_ALL);
        }

        private void ApplyNoSessionState()
        {
            HasSession = false;
            IsPlayerTurn = false;
            HeaderText = "No active session";
            PartySummaryText = "Party: —";
            EnemySummaryText = "Enemies: —";
            ActorSelectorText = "Actor: —";
            ActionSelectorText = "Action: —";
            TargetSelectorText = "Target: —";
            LogText = "Press 'Start demo encounter' to begin.";
            _cachedLogCount = 0;
            ResetSelectors();
        }

        private void ResetSelectors()
        {
            _actorIndex = 0;
            _actionIndex = 0;
            _targetIndex = 0;
        }

        private void RebuildActorOptions()
        {
            _actorOptions.Clear();
            if (!HasSession || _service?.Session?.Sides == null || _service.Session.Sides.Count == 0)
                return;

            CursorBattlePrototypeSide playerSide = _service.Session.Sides[0];
            if (playerSide?.Actors == null)
                return;

            for (int i = 0; i < playerSide.Actors.Count; i++)
            {
                CursorBattlePrototypeActor actor = playerSide.Actors[i];
                if (actor != null && actor.Character != null && !actor.Character.IsDead)
                    _actorOptions.Add(actor);
            }

            if (_actorOptions.Count == 0)
                _actorIndex = 0;
            else
                _actorIndex = WrapIndex(_actorIndex, _actorOptions.Count);
        }

        private void RebuildActionAndTargetOptions()
        {
            RebuildActionOptions();
            RebuildTargetOptions();
        }

        private void RebuildActionOptions()
        {
            _actionOptions.Clear();
            if (_actorOptions.Count == 0 || _service == null)
                return;

            CursorBattlePrototypeActor actor = _actorOptions[_actorIndex];
            List<CursorBattlePrototypeActionPreview> actions = _service.GetAvailableActions(actor.RuntimeId);
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] != null)
                    _actionOptions.Add(actions[i]);
            }

            if (_actionOptions.Count == 0)
                _actionIndex = 0;
            else
                _actionIndex = WrapIndex(_actionIndex, _actionOptions.Count);
        }

        private void RebuildTargetOptions()
        {
            _targetOptions.Clear();
            if (_actorOptions.Count == 0 || _actionOptions.Count == 0 || _service == null)
                return;

            int actorRuntimeId = _actorOptions[_actorIndex].RuntimeId;
            string actionId = _actionOptions[_actionIndex].ActionId;
            List<CursorBattlePrototypeActor> targets = _service.GetTargetCandidates(actorRuntimeId, actionId);
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null && targets[i].Character != null && !targets[i].Character.IsDead)
                    _targetOptions.Add(targets[i]);
            }

            if (_targetOptions.Count == 0)
                _targetIndex = 0;
            else
                _targetIndex = WrapIndex(_targetIndex, _targetOptions.Count);
        }

        private void UpdateSelectorTexts()
        {
            if (_actorOptions.Count == 0)
                ActorSelectorText = "Actor: —";
            else
            {
                CursorBattlePrototypeActor actor = _actorOptions[_actorIndex];
                int hp = GetHitPoints(actor.Character);
                ActorSelectorText = $"Actor [{_actorIndex + 1}/{_actorOptions.Count}]: {actor.Character.Name} (HP {hp})";
            }

            if (_actionOptions.Count == 0)
                ActionSelectorText = "Action: —";
            else
            {
                CursorBattlePrototypeActionPreview action = _actionOptions[_actionIndex];
                ActionSelectorText = $"Action [{_actionIndex + 1}/{_actionOptions.Count}]: {action.Title} (Cost {action.ActionCost})";
            }

            if (_targetOptions.Count == 0)
                TargetSelectorText = "Target: —";
            else
            {
                CursorBattlePrototypeActor target = _targetOptions[_targetIndex];
                int hp = GetHitPoints(target.Character);
                TargetSelectorText = $"Target [{_targetIndex + 1}/{_targetOptions.Count}]: {target.Character.Name} (HP {hp})";
            }
        }

        private static int WrapIndex(int index, int count)
        {
            if (count <= 0)
                return 0;

            while (index < 0)
                index += count;
            while (index >= count)
                index -= count;
            return index;
        }

        private static int GetHitPoints(CharacterStateData character)
        {
            if (character?.Parameters == null)
                return 0;

            return character.Parameters.TryGetValue(Glossary.Characters.HIT_POINTS, out int hp) ? hp : 0;
        }

        private static string BuildSideSummary(CursorBattlePrototypeSession session, int sideIndex, string fallbackTitle)
        {
            if (session?.Sides == null || sideIndex < 0 || sideIndex >= session.Sides.Count)
                return $"{fallbackTitle}: —";

            CursorBattlePrototypeSide side = session.Sides[sideIndex];
            if (side?.Actors == null || side.Actors.Count == 0)
                return $"{fallbackTitle}: —";

            var sb = new StringBuilder();
            sb.AppendLine(side.Id);
            for (int i = 0; i < side.Actors.Count; i++)
            {
                CursorBattlePrototypeActor actor = side.Actors[i];
                if (actor?.Character == null)
                    continue;

                int hp = GetHitPoints(actor.Character);
                string status = actor.Character.IsDead ? "DEAD" : "ALIVE";
                sb.AppendLine($"- {actor.Character.Name}: HP {hp} ({status})");
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildCompactLog(CursorBattlePrototypeSession session)
        {
            if (session?.Log == null || session.Log.Count == 0)
                return "Log is empty.";

            int maxLines = 14;
            int start = Mathf.Max(0, session.Log.Count - maxLines);
            var sb = new StringBuilder();
            for (int i = start; i < session.Log.Count; i++)
            {
                CursorBattlePrototypeLogEntry entry = session.Log[i];
                sb.Append('[').Append(entry.TimeUtc.ToString("HH:mm:ss")).Append("] ");
                sb.AppendLine(entry.Message);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
