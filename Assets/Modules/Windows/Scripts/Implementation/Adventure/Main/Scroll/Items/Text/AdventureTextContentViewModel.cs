using Modules.RPG.Scripts.Adventure.Data;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Text
{
    /// <summary>
    /// ViewModel for <see cref="SceneContentType.Text"/>.
    /// <see cref="SceneContentData.Value"/> is the display string (or localization key resolved before <see cref="Init"/>).
    /// </summary>
    public sealed class AdventureTextContentViewModel : AdventureContentViewModelBase
    {
        public string Text { get; private set; }

        public override void Init(SceneContentData data)
        {
            base.Init(data);
            Text = data.Value ?? string.Empty;
        }

        public override void Dispose()
        {
        }
    }
}
