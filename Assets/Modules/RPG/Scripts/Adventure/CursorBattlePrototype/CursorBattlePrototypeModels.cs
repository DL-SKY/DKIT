using Modules.Definitions.Scripts.Implementation.Adventures.Defs.BattleActions;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.CursorBattlePrototype
{
    public enum CursorBattlePrototypeResultType
    {
        None = 0,
        Victory = 1,
        Defeat = 2,
    }

    public sealed class CursorBattlePrototypeActor
    {
        public int RuntimeId;
        public int SourceCharacterId;
        public string SourceCreatureId;
        public bool IsPlayerSide;
        public CharacterStateData Character;
        public List<string> BattleActionIds = new List<string>();
    }

    public sealed class CursorBattlePrototypeSide
    {
        public string Id;
        public bool IsPlayerControlled;
        public List<CursorBattlePrototypeActor> Actors = new List<CursorBattlePrototypeActor>();
    }

    public sealed class CursorBattlePrototypeLogEntry
    {
        public DateTime TimeUtc;
        public string Message;
    }

    public sealed class CursorBattlePrototypeSession
    {
        public bool IsActive;
        public int Round;
        public int CurrentSideIndex;
        public int RemainingActions;
        public int ActionsPerTurn;
        public CursorBattlePrototypeResultType ResultType;
        public List<CursorBattlePrototypeSide> Sides = new List<CursorBattlePrototypeSide>();
        public List<CursorBattlePrototypeLogEntry> Log = new List<CursorBattlePrototypeLogEntry>();

        public readonly Dictionary<int, int> AttackCounterPerActor = new Dictionary<int, int>();
    }

    public sealed class CursorBattlePrototypeActionPreview
    {
        public string ActionId;
        public string Title;
        public int ActionCost;
        public BattleActionTargetType TargetType;
        public bool IsAttack;
    }
}
