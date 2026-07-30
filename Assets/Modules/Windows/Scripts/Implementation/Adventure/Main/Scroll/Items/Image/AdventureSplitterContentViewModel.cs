using Modules.RPG.Scripts.Adventure.Data;
using Modules.State.Scripts.Implementation.Adventure;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// ViewModel for <see cref="Modules.RPG.Scripts.Adventure.Data.SceneContentType.Splitter"/>.
    /// No path/display data — the prefab holds a static image; appearance is handled by the view animator.
    /// </summary>
    public sealed class AdventureSplitterContentViewModel : AdventureContentViewModelBase
    {
        [Inject] private readonly AdventureStateManager _stateManager;

        /// <summary>
        /// Snapshot of current adventure / scene ids at VM creation time (for debug UI).
        /// </summary>
        public string DebugText { get; private set; } = string.Empty;

        public override void Init(SceneContentData data)
        {
            base.Init(data);

            AdventuresStateData adventuresState = _stateManager?.State?.Adventures;
            string adventureId = adventuresState?.CurrentAdventureId ?? string.Empty;
            string sceneId = adventuresState?.CurrentAdventureSceneId ?? string.Empty;
            DebugText = $"{adventureId} -> {sceneId}";
        }
    }
}
