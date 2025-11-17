using Leopotam.Ecs;
using Modules.Match3.Scripts.Helpers;
using Modules.Match3.Scripts.Interfaces;
using UnityEngine;

namespace Modules.ECS.Scripts.Match3.Systems
{
    /// <summary>
    /// Система настройки камеры в зависимости от размеров игрового поля.
    /// Автоматически настраивает камеру так, чтобы все игровое поле помещалось на экране.
    /// </summary>
    public class CameraSetupSystem : IEcsInitSystem
    {
        private const float EDGE_SPACING_MOD = 1.125f;

        private readonly IGameZoneData _gameZoneData;

        public CameraSetupSystem(IGameZoneData gameZoneData)
        {
            _gameZoneData = gameZoneData;
        }

        public void Init()
        {
            if (_gameZoneData == null)
            {
                UnityEngine.Debug.LogError($"[CameraSetupSystem] GameZoneData is null!");
                return;
            }

            var mask = _gameZoneData.GetMask();
            if (mask == null)
            {
                UnityEngine.Debug.LogError($"[CameraSetupSystem] Mask is null!");
                return;
            }

            // Получаем главную камеру
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                UnityEngine.Debug.LogError($"[CameraSetupSystem] Main camera not found!");
                return;
            }

            // Вычисляем размеры игрового поля
            int rows = mask.GetLength(0);  // количество строк в маске
            int cols = mask.GetLength(1);  // количество столбцов в маске
            int width = cols;  // ширина поля в Unity (столбцы)
            int height = rows; // высота поля в Unity (строки)

            // Настраиваем камеру в зависимости от типа проекции
            if (mainCamera.orthographic)
            {
                // Для ортографической камеры настраиваем orthographic size
                float orthographicSize = GridPositionHelper.CalculateCameraOrthographicSize(width, height, EDGE_SPACING_MOD);
                mainCamera.orthographicSize = orthographicSize;
                
                // Центрируем камеру на игровом поле
                mainCamera.transform.position = new Vector3(0, 0, mainCamera.transform.position.z);
                
                UnityEngine.Debug.Log($"[CameraSetupSystem] Orthographic camera set up: size={orthographicSize}, field size={width}x{height}");
            }
            else
            {
                // Для перспективной камеры вычисляем расстояние и позицию
                // Используем угол обзора для вычисления необходимого расстояния
                float fieldOfView = mainCamera.fieldOfView;
                float aspectRatio = (float)Screen.width / Screen.height;
                
                // Вычисляем размеры поля в мировых единицах
                float fieldWidth = width;
                float fieldHeight = height;
                
                // Вычисляем необходимое расстояние для перспективной камеры
                // Используем формулу: distance = (fieldSize / 2) / tan(FOV / 2)
                float distanceByWidth = (fieldWidth / 2f) / Mathf.Tan(fieldOfView * Mathf.Deg2Rad / 2f) / aspectRatio;
                float distanceByHeight = (fieldHeight / 2f) / Mathf.Tan(fieldOfView * Mathf.Deg2Rad / 2f);
                float distance = Mathf.Max(distanceByWidth, distanceByHeight) * EDGE_SPACING_MOD; // добавляем небольшой запас
                
                // Устанавливаем позицию камеры
                // Камера смотрит на центр поля (0, 0, 0)
                mainCamera.transform.position = new Vector3(0, 0, -distance);
                mainCamera.transform.LookAt(Vector3.zero);
                
                UnityEngine.Debug.Log($"[CameraSetupSystem] Perspective camera set up: distance={distance}, field size={width}x{height}");
            }
        }
    }
}

