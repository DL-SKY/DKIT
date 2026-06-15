using Modules.Match3.Scripts.Implementation.Core;
using Zenject.Scripts.Factories;

namespace Zenject.Scripts.Installers
{
    public class Match3SceneInstaller : MonoInstaller<Match3SceneInstaller>
    {
        public override void InstallBindings()
        {
            //Core
            Container.BindInterfacesAndSelfTo<Match3RoundController>().AsSingle().NonLazy();    //IDisposable

            //Factories
            Container.Bind<EcsSystemFactory>().AsSingle();
            //...
        }
    }
}
