using Modules.RPG.Scripts.Adventure.Data;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Image
{
    /// <summary>
    /// ViewModel for <see cref="SceneContentType.RandomImage"/> — picks one entry from
    /// <see cref="SceneContentData.Values"/> once during <see cref="Init"/>.
    /// </summary>
    public sealed class AdventureRandomImageContentViewModel : AdventureImageContentViewModelBase
    {
        public override void Init(SceneContentData data)
        {
            InitImage(data, PickRandomPath(data));
        }

        private static string PickRandomPath(SceneContentData data)
        {
            if (data?.Values == null || data.Values.Count == 0)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(AdventureRandomImageContentViewModel)}] Values is empty; no image path selected.");
                return string.Empty;
            }

            int index = Random.Range(0, data.Values.Count);
            return data.Values[index];
        }
    }
}
