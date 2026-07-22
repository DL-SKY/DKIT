using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Items;
using Modules.RPG.Scripts.Adventure.Data;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Item
{
    /// <summary>
    /// ViewModel for <see cref="SceneContentType.Item"/>.
    /// <see cref="SceneContentData.Value"/> is the item id; resolved via <see cref="DefinitionsManager"/>.
    /// </summary>
    public sealed class AdventureItemContentViewModel : AdventureContentViewModelBase
    {
        [Inject] private readonly DefinitionsManager _definitionsManager;

        public string ItemId { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public override void Init(SceneContentData data)
        {
            base.Init(data);

            ItemId = data.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(ItemId)
                && _definitionsManager.Items != null
                && _definitionsManager.Items.TryGetValue(ItemId, out ItemDef itemDef)
                && itemDef != null)
            {
                Title = itemDef.Title ?? ItemId;
                Description = itemDef.Description ?? string.Empty;
            }
            else
            {
                Title = ItemId;
                Description = string.Empty;
            }
        }

        public override void Dispose()
        {
        }
    }
}
