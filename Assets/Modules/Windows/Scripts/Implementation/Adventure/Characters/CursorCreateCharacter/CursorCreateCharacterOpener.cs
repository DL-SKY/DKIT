using Modules.Windows.Scripts.Managers;
using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter
{
    /// <summary>
    /// Opens <see cref="CursorCreateCharacterView"/> without touching existing adventure wiring.
    /// </summary>
    public static class CursorCreateCharacterOpener
    {
        public static void Open()
        {
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CursorCreateCharacterOpener)}] Open only works in Play Mode.");
                return;
            }

            if (!ProjectContext.HasInstance)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(CursorCreateCharacterOpener)}] ProjectContext is not available.");
                return;
            }

            DiContainer container = ProjectContext.Instance.Container;
            var windowsManager = container.Resolve<WindowsManager>();
            var viewModelFactory = container.Resolve<ViewModelFactory>();

            var viewModel = viewModelFactory.Create<CursorCreateCharacterViewModel>();
            viewModel.Init();
            windowsManager.OpenView<CursorCreateCharacterView, CursorCreateCharacterViewModel>(
                CursorCreateCharacterView.Path,
                viewModel);
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Cursor/Open Create Character Window")]
        private static void OpenFromMenu()
        {
            Open();
        }
#endif
    }
}
