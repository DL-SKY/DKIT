using System;
using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Общая модель метки на экране. Свой корабль — <see cref="SpacePlayerMarkerViewModel"/>.
    /// Чужой корабль — <see cref="SpaceShipMarkerViewModel"/>.
    /// Остальные цели — <see cref="SpaceSectorMarkerViewModel"/>.
    /// Картинку выбирает наследник. Какие кнопки стоят на круге, тоже решает наследник.
    /// Сейчас список у всех наследников пустой: круг не показывается.
    /// Вид рисует <see cref="MenuButtons"/> и сам кнопки не добавляет и не выбрасывает.
    /// <para>
    /// Скорость, радиус и прочность не приходят событием. Их читают с сущности сектора в том же
    /// обходе, что уже делает <see cref="SpaceSessionHost.CopyMarkers"/> каждый кадр.
    /// Корабль игрока — сущность с <c>SpacePlayerHull</c>. Остальное тело узнаётся по
    /// <c>SpaceLabel.Value</c> = <see cref="DefId"/> и по <c>SpaceKind</c> = <see cref="Kind"/>.
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
        private Action<SpaceMarkerViewModel, string> _onCommand;
        private bool _menuOpen;

        public string TargetId { get; private set; }
        public string DefId { get; private set; }
        public bool Ship { get; private set; }
        public bool Player { get; private set; }
        public SpaceObjectKind Kind { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public float MetersPerSecond { get; private set; }
        public float MaxMetersPerSecond { get; private set; }
        public string IconResourcesPath { get; private set; }
        public bool MenuOpen => _menuOpen;
        public IReadOnlyList<SpaceMarkerMenuButton> MenuButtons => _menuButtons;
        public virtual bool ShowSpeed => true;

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

        public void ClickCommand(string commandId)
        {
            _onCommand?.Invoke(this, commandId);
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
            Action<SpaceMarkerViewModel, string> onCommand)
        {
            TargetId = snapshot.TargetId;
            DefId = snapshot.DefId;
            Ship = snapshot.Ship;
            Player = snapshot.Player;
            Kind = snapshot.Kind;
            WorldPosition = snapshot.WorldPosition;
            MetersPerSecond = snapshot.MetersPerSecond;
            MaxMetersPerSecond = snapshot.MaxMetersPerSecond;
            IconResourcesPath = iconResourcesPath ?? string.Empty;
            _onClick = onClick;
            _onCommand = onCommand;
            _menuButtons.Clear();
        }

        protected void AddMenuButtons(SpaceDefinitionsManager definitions, List<string> commandIds)
        {
            if (commandIds == null)
                return;

            for (var i = 0; i < commandIds.Count; i++)
            {
                var commandId = commandIds[i];
                if (string.IsNullOrEmpty(commandId) || HasMenuButton(commandId))
                    continue;

                if (definitions == null || !definitions.TryGetCommand(commandId, out _))
                    continue;

                _menuButtons.Add(new SpaceMarkerMenuButton(commandId, commandId));
            }
        }

        private bool HasMenuButton(string commandId)
        {
            for (var i = 0; i < _menuButtons.Count; i++)
            {
                if (_menuButtons[i].CommandId == commandId)
                    return true;
            }

            return false;
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

        protected static bool TryGetHullCommands(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
            out List<string> commandIds)
        {
            commandIds = null;
            if (definitions == null || !definitions.TryGetHull(snapshot.DefId, out var hull) || hull == null)
                return false;

            commandIds = hull.CommandIds;
            return true;
        }
    }
}
