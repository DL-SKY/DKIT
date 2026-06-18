using Modules.Windows.Scripts.Base;
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

        protected override void InitImplementation()
        {
            // Стартовое состояние UI из _viewModel
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
