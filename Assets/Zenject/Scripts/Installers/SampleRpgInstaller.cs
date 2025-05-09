using UnityEngine;
using Zenject;

public class SampleRpgInstaller : Installer<SampleRpgInstaller>
{
    public override void InstallBindings()
    {
        /*
        https://rutube.ru/video/ad1aa3b8c06a71b831078e61c62ddc23/?ysclid=mah8l6voh5379306188




        public override void InstallBindings()
        {
            Container.Bind<Player>().AsSingle(); // ����������� ������ Player
            Container.BindInterfacesTo<InputHandler>().AsSingle(); // ��������� + ����������
        }




        [Inject] private Health _health; // ���� ����� ��������� �������������




        //�������
        public class EnemyFactory : PlaceholderFactory<Enemy> { }

        public class GameInstaller : MonoInstaller {
            public override void InstallBindings() {
                Container.BindFactory<Enemy, EnemyFactory>().FromComponentInNewPrefab(enemyPrefab);
            }
        }

        // �������������:
        [Inject] private EnemyFactory _enemyFactory;
        void SpawnEnemy() {
            Enemy enemy = _enemyFactory.Create(); // ������� ������ Enemy � DI
        }




        //��������� �������
        public class CustomPlayerViewModelFactory : IFactory<PlayerModel, PlayerViewModel> {
        private readonly DiContainer _container;

        public CustomPlayerViewModelFactory(DiContainer container) {
            _container = container;
        }

        public PlayerViewModel Create(PlayerModel model) {
            var vm = _container.Instantiate<PlayerViewModel>(new object[] { model });
            // �������������� ��������� ViewModel
            return vm;
        }
    }



        //������� ��� VM
        public interface IPlayerViewModelFactory {
            PlayerViewModel Create(PlayerModel model);
        }

        public class PlayerViewModelFactory : PlaceholderFactory<PlayerModel, PlayerViewModel> { }
        //����������: PlaceholderFactory � ������� ���������� Zenject ��� ������.



        */
    }
}