using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Components;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main
{
    /// <summary>
    /// Заготовка V для главного экрана Adventure: префаб должен лежать под Resources по пути <see cref="Path"/>.
    /// </summary>
    public class AdventureMainView : ViewBase<AdventureMainViewModel>
    {
        public static string Path = "Prefabs/Views/Adventure/AdventureMainView";

        // [SerializeField] private ... ссылки на UI-элементы
        [SerializeField] private CachedPathImage _image;

        protected override void InitImplementation()
        {
            // Стартовое состояние UI из _viewModel



            //TODO: test
            //https://i.pinimg.com/originals/92/b7/e1/92b7e1ef89260809295c10012f833acd.jpg
            _image.SetPath("https://static.vecteezy.com/system/resources/thumbnails/020/617/592/large/loading-circle-animation-on-black-transparent-background-with-alpha-channel-element-animation-for-web-interface-or-application-interface-and-more-searching-updating-and-buffering-circle-icon-free-video.jpg");
            _image.SetPath("https://i.pinimg.com/originals/33/93/41/339341a3ebc61e65624eb522a19a1722.jpg");
            _image.SetPath("https://i.pinimg.com/originals/92/b7/e1/92b7e1ef89260809295c10012f833acd.jpg");
        }

        protected override void Subscribe()
        {
            // _viewModel.OnChange += ...
            // _viewModel.OnChangeCustom += ...
        }

        protected override void Unsubscribe()
        {
            // снять все подписки из Subscribe
        }

        public override void Show()
        {
            base.Show();
            // При необходимости — показ панели, анимация
        }
    }
}
