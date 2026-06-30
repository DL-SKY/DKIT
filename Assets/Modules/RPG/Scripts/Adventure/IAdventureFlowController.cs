using System;

namespace Modules.RPG.Scripts.Adventure
{
    [Obsolete]
    public interface IAdventureFlowController
    {
        void GoToScene(string sceneId);
    }
}
