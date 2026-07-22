using Modules.RPG.Scripts.Adventure.Data;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// ViewModel for <see cref="SceneContentType.Image"/> — single path/URL in <see cref="SceneContentData.Value"/>.
    /// </summary>
    public class AdventureImageContentViewModel : AdventureImageContentViewModelBase
    {
        public override void Init(SceneContentData data)
        {
            InitImage(data, data?.Value);
        }

    }
}
