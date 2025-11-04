namespace Zenject.Scripts.Installers
{
    public class Match3SceneInstaller : MonoInstaller<Match3SceneInstaller>
    {
        public override void InstallBindings()
        {
            //Debug
            Container.Bind<M3Prop>().AsSingle().NonLazy();
        }
    }

    public class M3Prop
    { 
        public int Count { get; private set; }
    }
}
