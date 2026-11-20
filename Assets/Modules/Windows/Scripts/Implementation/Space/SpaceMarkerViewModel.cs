using System;
using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Session;
using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Space
{
    /// <summary>
    /// Одна метка на экране. Картинка и круг команд берутся из дефа цели.
    /// Картинка корабля — <c>SpaceHullDef.IconResourcesPath</c>. Остальные типы читают картинку
    /// из единственного <c>SpaceMarkerIconsDef</c>.
    /// <para>
    /// Скорость, радиус и прочность не приходят событием. Их читают с сущности сектора в том же
    /// обходе, что уже делает <see cref="SpaceSessionHost.CopyMarkers"/> каждый кадр.
    /// Корабль игрока — сущность с <c>SpacePlayerHull</c>. Остальное тело узнаётся по
    /// <c>SpaceLabel.Value</c> = <see cref="DefId"/> и по <c>SpaceKind</c> = <see cref="Kind"/>.
    /// </para>
    /// <para>
    /// Текущая скорость — <c>SpaceMotion.MetersPerSecond</c>, метры в секунду вдоль носа.
    /// Тестовая подпись метки уже копирует это число в <see cref="MetersPerSecond"/>.
    /// Если компонента нет, скорость цели равна нулю: так сейчас у неподвижных тел.
    /// <c>MaxMetersPerSecond</c> и <c>InertiaSeconds</c> — потолок и время набора, их при спавне
    /// копируют с надетого двигателя. В дефе корпуса скорости нет. Она лежит на дефе двигателя:
    /// <c>SpaceModuleDef.MaxSpeedMetersPerSecond</c> и <c>InertiaSeconds</c>, модуль находится
    /// по слоту в надетом оснащении профиля. Скорость поворота, если метке понадобится, —
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
    public sealed class SpaceMarkerViewModel : ViewModelBase
    {
        public const string ON_MENU = "menu";

        private readonly List<string> _commands = new List<string>();
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
        public string IconResourcesPath { get; private set; }
        public bool MenuOpen => _menuOpen;
        public IReadOnlyList<string> Commands => _commands;

        public void Init(
            SpaceMarkerSnapshot snapshot,
            SpaceDefinitionsManager definitions,
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
            IconResourcesPath = ReadIconPath(snapshot, definitions);
            _onClick = onClick;
            _onCommand = onCommand;
            _commands.Clear();
            CollectCommands(snapshot, definitions);
        }

        public void SetLive(Vector3 worldPosition, float metersPerSecond)
        {
            WorldPosition = worldPosition;
            MetersPerSecond = metersPerSecond;
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
            _commands.Clear();
            _menuOpen = false;
        }

        private void CollectCommands(SpaceMarkerSnapshot snapshot, SpaceDefinitionsManager definitions)
        {
            if (definitions == null || string.IsNullOrEmpty(snapshot.DefId))
                return;

            if (snapshot.Ship)
            {
                if (definitions.TryGetHull(snapshot.DefId, out var hull))
                    CopyCommands(hull?.CommandIds);
                return;
            }

            if (definitions.TryGetObject(snapshot.DefId, out var body))
                CopyCommands(body?.CommandIds);
        }

        private void CopyCommands(List<string> commandIds)
        {
            if (commandIds == null)
                return;

            for (var i = 0; i < commandIds.Count; i++)
            {
                var commandId = commandIds[i];
                if (string.IsNullOrEmpty(commandId) || _commands.Contains(commandId))
                    continue;

                _commands.Add(commandId);
            }
        }

        private static string ReadIconPath(SpaceMarkerSnapshot snapshot, SpaceDefinitionsManager definitions)
        {
            if (snapshot.Ship)
            {
                if (definitions != null
                    && definitions.TryGetHull(snapshot.DefId, out var hull)
                    && hull != null
                    && !string.IsNullOrWhiteSpace(hull.IconResourcesPath))
                    return hull.IconResourcesPath;

                UnityEngine.Debug.LogError($"[Space] Ship marker '{snapshot.DefId}' has no icon on its hull def.");
                return string.Empty;
            }

            if (definitions?.MarkerIcons != null && definitions.MarkerIcons.TryGetIcon(snapshot.Kind, out var path))
                return path;

            UnityEngine.Debug.LogError($"[Space] Marker icons have no picture for {snapshot.Kind}.");
            return string.Empty;
        }
    }
}
