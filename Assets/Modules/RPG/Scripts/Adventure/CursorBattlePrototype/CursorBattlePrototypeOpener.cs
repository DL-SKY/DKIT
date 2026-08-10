using UnityEngine;
using Zenject;
using Zenject.Scripts.Factories;
using Modules.Windows.Scripts.Implementation.Adventure.CursorBattlePrototype;
using Modules.Windows.Scripts.Managers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Modules.RPG.Scripts.Adventure.CursorBattlePrototype
{
    /// <summary>
    /// Opens the prototype combat window in Play Mode.
    /// </summary>
    public static class CursorBattlePrototypeOpener
    {
        public static void Open()
        {
            OpenInternal(autoStartDemoEncounter: false);
        }

        public static void OpenFromCheats()
        {
            OpenInternal(autoStartDemoEncounter: true);
        }

        private static void OpenInternal(bool autoStartDemoEncounter)
        {
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.LogWarning("[CursorBattlePrototypeOpener] Open is available only in Play Mode.");
                return;
            }

            if (!ProjectContext.HasInstance)
            {
                UnityEngine.Debug.LogWarning("[CursorBattlePrototypeOpener] ProjectContext is not available.");
                return;
            }

            DiContainer container = ProjectContext.Instance.Container;
            WindowsManager windowsManager = container.Resolve<WindowsManager>();
            ViewModelFactory viewModelFactory = container.Resolve<ViewModelFactory>();
            CursorBattlePrototypeViewModel viewModel = viewModelFactory.Create<CursorBattlePrototypeViewModel>();
            viewModel.Init();

            if (autoStartDemoEncounter)
                viewModel.StartDemoEncounterIfNeeded();

            windowsManager.OpenView<CursorBattlePrototypeWindow, CursorBattlePrototypeViewModel>(
                CursorBattlePrototypeWindow.Path,
                viewModel);
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Cursor/Open Battle Prototype Window")]
        private static void OpenFromMenu()
        {
            Open();
        }

        [MenuItem("Tools/Cheats/Open Battle Prototype Window")]
        private static void OpenFromCheatsMenu()
        {
            OpenFromCheats();
        }
#endif
    }
}
