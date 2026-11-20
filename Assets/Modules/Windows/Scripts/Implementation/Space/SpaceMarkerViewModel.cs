using System;
using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Space.Scripts.Implementation.State;
using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Общая модель метки на экране. Свой корабль — <see cref="SpacePlayerMarkerViewModel"/>.
    /// Чужой корабль — <see cref="SpaceShipMarkerViewModel"/>.
    /// Остальные цели — <see cref="SpaceSectorMarkerViewModel"/>.
    /// Картинку выбирает наследник. Кнопки круга собирает база из корпуса игрока и надетых модулей.
    /// Команда видна, только если тип и отношение этой метки есть в её дефе.
    /// Вид рисует <see cref="MenuButtons"/> и сам кнопки не добавляет и не выбрасывает.
    /// <para>
    /// Скорость, радиус и прочность не приходят событием. Их читают с сущности сектора в том же
    /// обходе, что уже делает <see cref="SpaceSessionHost.CopyMarkers"/> каждый кадр.
    /// Корабль игрока — сущность с <c>SpacePlayerHull</c>. Остальное тело узнаётся по
    /// <c>SpaceLabel.Value</c> = <see cref="DefId"/> и по <c>SpaceBodyType</c> = <see cref="ObjectType"/>.
    /// </para>
    /// <para>
    /// Текущая скорость — <c>SpaceMotion.MetersPerSecond</c>, метры в секунду вдоль носа.
    /// Потолок — <c>SpaceMotion.MaxMetersPerSecond</c>. Тестовая подпись корабля копирует оба числа:
    /// текущую скорость и в скобках процент от потолка. Секторная метка подпись скорости не показывает.
    /// Если компонента нет, скорость цели равна нулю: так сейчас у неподвижных тел.
    /// <c>MaxMetersPerSecond</c> и <c>InertiaSeconds</c> — потолок и время набора, их при спавне
    /// копируют со строки надетого двигателя для типоразмера гнезда. В дефе корпуса скорости нет.
    /// Строки лежат в <c>SpaceModuleDef.SizeParameters</c>, модуль находится по слоту в надетом
    /// оснащении профиля. Скорость поворота, если метке понадобится, —
    /// <c>SpaceTurnRate.DegreesPerSecond</c> из <c>SpaceHullDef.ManeuverabilityDegreesPerSecond</c>.
    /// </para>
    /// <para>
    /// Радиус в метрах — <c>SpaceRadius.Meters</c> на той же сущности. Кораблю его копируют из
    /// <c>SpaceHullDef.Radius</c>, телу — из <c>SpaceObjectDef.Radius</c>. Те же числа остаются
    /// в дефах: <c>TryGetHull</c> и <c>TryGetObject</c> по <see cref="DefId"/>. Ноль значит, что
    /// тело никого не отталкивает. Дистанция до игрока — разница между <see cref="WorldPosition"/>
    /// и <c>SpacePosition</c> корпуса игрока.
    /// </para>
    /// <para>
    /// Прочности корпуса и щитов нет ни на сущности, ни в дефе корпуса или объекта.
    /// Их нельзя выводить из радиуса или из двигателя. Когда появятся, текущие значения
    /// должны лежать на сущности, а максимумы — в дефе корпуса или модуля.
    /// </para>
    /// </summary>
    public abstract class SpaceMarkerViewModel : ViewModelBase
    {
        public const string ON_MENU = "menu";

        private readonly List<SpaceMarkerMenuButton> _menuButtons = new List<SpaceMarkerMenuButton>();
        private Action<SpaceMarkerViewModel> _onClick;
        private Action<SpaceMarkerViewModel, string, string> _onCommand;
        private bool _menuOpen;

        public string TargetId { get; private set; }
        public string DefId { get; private set; }
        public bool Ship { get; private set; }
        public bool Player { get; private set; }
        public SpaceObjectType ObjectType { get; private set; }
        public SpaceRelation Relation { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public float MetersPerSecond { get; private set; }
        public float MaxMetersPerSecond { get; private set; }
        public string IconResourcesPath { get; private set; }
        public bool MenuOpen => _menuOpen;
        public float MenuRadius { get; private set; } = 78f;
        public IReadOnlyList<SpaceMarkerMenuButton> MenuButtons => _menuButtons;
        public virtual bool ShowSpeed => true;

        public void SetMenuRadius(float radius)
        {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                return;

            MenuRadius = radius;
        }

        public void SetLive(Vector3 worldPosition, float metersPerSecond, float maxMetersPerSecond)
        {
            WorldPosition = worldPosition;
            MetersPerSecond = metersPerSecond;
            MaxMetersPerSecond = maxMetersPerSecond;
        }

        public void SetMenuOpen(bool open)
        {
            if (_menuOpen == open)
                return;

            _menuOpen = open;
            SendOnChange(ON_MENU);
        }

        public void Click()
        {
            _onClick?.Invoke(this);
        }

        public void ClickCommand(string commandId, string sourceId)
        {
            _onCommand?.Invoke(this, commandId, sourceId);
        }

        public override void Dispose()
        {
            _onClick = null;
            _onCommand = null;
            _menuButtons.Clear();
            _menuOpen = false;
        }

        protected void Bind(
            SpaceMarkerSnapshot snapshot,
            string iconResourcesPath,
            Action<SpaceMarkerViewModel> onClick,
            Action<SpaceMarkerViewModel, string, string> onCommand)
        {
            TargetId = snapshot.TargetId;
            DefId = snapshot.DefId;
            Ship = snapshot.Ship;
            Player = snapshot.Player;
            ObjectType = snapshot.ObjectType;
            Relation = snapshot.Relation;
            WorldPosition = snapshot.WorldPosition;
            MetersPerSecond = snapshot.MetersPerSecond;
            MaxMetersPerSecond = snapshot.MaxMetersPerSecond;
            IconResourcesPath = iconResourcesPath ?? string.Empty;
            _onClick = onClick;
            _onCommand = onCommand;
            _menuButtons.Clear();
        }

        protected void AddPlayerCommands(SpaceDefinitionsManager definitions, SpaceProfile profile)
        {
            var ship = profile != null ? profile.ActiveShip() : null;
            if (definitions == null || ship == null)
                return;

            var ranked = new List<RankedCommand>();
            if (definitions.TryGetHull(ship.HullId, out var hull) && hull != null)
                Collect(definitions, hull.CommandIds, hull.Id, ranked);

            var slots = ship.Slots;
            if (slots != null)
            {
                for (var i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (slot == null || string.IsNullOrEmpty(slot.ModuleId))
                        continue;

                    if (!definitions.TryGetModule(slot.ModuleId, out var module) || module == null)
                        continue;

                    Collect(definitions, module.CommandIds, module.Id, ranked);
                }
            }

            ranked.Sort(CompareCommands);
            for (var i = 0; i < ranked.Count; i++)
                _menuButtons.Add(new SpaceMarkerMenuButton(ranked[i].Id, ranked[i].SourceId, ranked[i].Label, ranked[i].IconResourcesPath));
        }

        private void Collect(SpaceDefinitionsManager definitions, List<string> commandIds, string sourceId, List<RankedCommand> ranked)
        {
            if (commandIds == null)
                return;

            for (var i = 0; i < commandIds.Count; i++)
            {
                var commandId = commandIds[i];
                if (string.IsNullOrEmpty(commandId) || Contains(ranked, commandId))
                    continue;

                if (!definitions.TryGetCommand(commandId, out var command) || command == null)
                    continue;

                if (!command.Allows(ObjectType, Relation))
                    continue;

                ranked.Add(new RankedCommand
                {
                    Id = commandId,
                    SourceId = sourceId,
                    Priority = command.PriorityOrder,
                    Label = string.IsNullOrEmpty(command.Name) ? commandId : command.Name,
                    IconResourcesPath = command.IconResourcesPath
                });
            }
        }

        private static bool Contains(List<RankedCommand> ranked, string commandId)
        {
            for (var i = 0; i < ranked.Count; i++)
            {
                if (ranked[i].Id == commandId)
                    return true;
            }

            return false;
        }

        private static int CompareCommands(RankedCommand left, RankedCommand right)
        {
            var order = left.Priority.CompareTo(right.Priority);
            if (order != 0)
                return order;

            return string.CompareOrdinal(left.Id, right.Id);
        }

        private struct RankedCommand
        {
            public string Id;
            public string SourceId;
            public int Priority;
            public string Label;
            public string IconResourcesPath;
        }

        protected static string ReadHullIcon(SpaceMarkerSnapshot snapshot, SpaceDefinitionsManager definitions)
        {
            if (definitions != null
                && definitions.TryGetHull(snapshot.DefId, out var hull)
                && hull != null
                && !string.IsNullOrWhiteSpace(hull.IconResourcesPath))
                return hull.IconResourcesPath;

            UnityEngine.Debug.LogError($"[Space] Ship marker '{snapshot.DefId}' has no icon on its hull def.");
            return string.Empty;
        }
    }
}
