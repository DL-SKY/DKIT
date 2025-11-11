using Modules.Match3.Scripts.Implementation.Core;

namespace Zenject.Scripts.Installers
{
    public class Match3SceneInstaller : MonoInstaller<Match3SceneInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Match3RoundController>().AsSingle().NonLazy();    //IDisposable
        }
    }
}
