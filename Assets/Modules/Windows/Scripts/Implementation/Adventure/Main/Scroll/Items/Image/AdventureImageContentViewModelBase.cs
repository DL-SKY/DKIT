using Modules.RPG.Scripts.Adventure.Data;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// Shared ViewModel state for image-like content (<see cref="SceneContentType.Image"/>,
    /// <see cref="SceneContentType.RandomImage"/>, <see cref="SceneContentType.Slideshow"/>,
    /// <see cref="SceneContentType.Splitter"/>).
    /// Holds only the image path/URL — sprite loading is the View's responsibility.
    /// </summary>
    public abstract class AdventureImageContentViewModelBase : AdventureContentViewModelBase
    {
        public const string ON_CHANGE_PATH = "ON_CHANGE_PATH";

        /// <summary>
        /// Resources path or remote URL of the image to display.
        /// </summary>
        public string CurrentPath { get; private set; }

        protected void InitImage(SceneContentData data, string initialPath)
        {
            base.Init(data);
            SetPath(initialPath, notify: false);
        }

        protected void SetPath(string path, bool notify)
        {
            CurrentPath = path ?? string.Empty;

            if (notify)
                SendOnChange(ON_CHANGE_PATH);
        }
    }
}
