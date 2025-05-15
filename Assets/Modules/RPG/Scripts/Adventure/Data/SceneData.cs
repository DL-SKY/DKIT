using Modules.RPG.Scripts.Adventure.Choice;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Data
{
    public class SceneData
    {
        public string Id;
        public List<string> Tags;

        public List<SceneContentData> Content;

        public bool NotClearScene;

        public List<ChoiceData> Choices;
    }
}
