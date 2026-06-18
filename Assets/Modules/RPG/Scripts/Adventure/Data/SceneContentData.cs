using Modules.Restrictions.Scripts.Core;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Data
{
    public enum SceneContentType
    {
        Text = 0,

        Image = 10,

        Splitter = 20,

        Item = 30,
    }


    public class SceneContentData
    {
        public SceneContentType Type;

        public List<Restriction> Restrictions;

        public string Value;
    }
}
