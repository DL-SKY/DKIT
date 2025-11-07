using UnityEngine;
using UnityEditor;
using System.IO;

namespace Modules.Utils.Scripts.Editor.Screenshot
{
    public class TransparentScreenshotToolWindow : EditorWindow
    {
        private Camera targetCamera;
        private int resolution = 512;
        private string fileName = "transparent_screenshot";
        private string savePath = "Assets/Screenshots/";
        private bool useMainCamera = true;


        [MenuItem("Tools/Transparent Screenshot Tool")]
        public static void ShowWindow()
        {
            GetWindow<TransparentScreenshotToolWindow>("Transparent Screenshot");
        }

        private void OnGUI()
        {
            GUILayout.Label("Transparent Screenshot Settings", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            useMainCamera = EditorGUILayout.Toggle("Use Main Camera", useMainCamera);

            if (!useMainCamera)
            {
                targetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", targetCamera, typeof(Camera), true);
            }

            resolution = EditorGUILayout.IntField("Resolution", resolution);
            fileName = EditorGUILayout.TextField("File Name", fileName);
            savePath = EditorGUILayout.TextField("Save Path", savePath);

            EditorGUILayout.Space();

            if (GUILayout.Button("Take Transparent Screenshot", GUILayout.Height(30)))
            {
                TakeTransparentScreenshot();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Instructions:\n1. Arrange your prefabs and objects in the scene\n2. Position camera as desired\n3. Click the button above\n4. Find your PNG in the specified folder", MessageType.Info);

            EditorGUILayout.HelpBox("Important: Make sure your camera background is set to Solid Color with alpha = 0", MessageType.Warning);
        }

        private void TakeTransparentScreenshot()
        {
            // Получаем камеру
            Camera camera = useMainCamera ? Camera.main : targetCamera;

            if (camera == null)
            {
                UnityEngine.Debug.LogError($"No camera found! Please assign a camera or ensure Main Camera exists in scene.");
                return;
            }

            // Сохраняем оригинальные настройки камеры
            CameraClearFlags originalClearFlags = camera.clearFlags;
            Color originalBackgroundColor = camera.backgroundColor;
            RenderTexture originalTargetTexture = camera.targetTexture;

            try
            {
                // Настраиваем камеру для прозрачного рендера
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0, 0, 0, 0); // Полностью прозрачный фон

                // Создаем RenderTexture с поддержкой альфа-канала
                RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
                renderTexture.Create();

                // Устанавливаем RenderTexture и рендерим
                camera.targetTexture = renderTexture;
                camera.Render();

                // Читаем данные из RenderTexture
                Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                texture.Apply();

                // Сохраняем как PNG
                SaveTextureAsPNG(texture);

                // Очищаем ресурсы
                DestroyImmediate(texture);
                renderTexture.Release();
                DestroyImmediate(renderTexture);

                UnityEngine.Debug.Log($"Transparent screenshot saved successfully!");
            }
            finally
            {
                // Восстанавливаем оригинальные настройки камеры
                camera.clearFlags = originalClearFlags;
                camera.backgroundColor = originalBackgroundColor;
                camera.targetTexture = originalTargetTexture;
                RenderTexture.active = null;
            }
        }

        private void SaveTextureAsPNG(Texture2D texture)
        {
            // Создаем папку если не существует
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            // Убеждаемся, что имя файла имеет правильное расширение
            string finalFileName = fileName;
            if (!finalFileName.ToLower().EndsWith(".png"))
            {
                finalFileName += ".png";
            }

            string fullPath = Path.Combine(savePath, finalFileName);

            // Конвертируем в PNG и сохраняем
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

            // Обновляем AssetDatabase
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log($"Screenshot saved to: {fullPath}");

            // Выделяем сохраненный файл в проекте
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            if (savedTexture != null)
            {
                EditorGUIUtility.PingObject(savedTexture);
                Selection.activeObject = savedTexture;
            }
        }
    }
}
