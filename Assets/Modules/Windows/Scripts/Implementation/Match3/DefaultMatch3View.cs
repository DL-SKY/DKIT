using Modules.Windows.Scripts.Base;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Match3
{
    /// <summary>
    /// Заготовка V для экрана Match3: префаб должен лежать под Resources по пути <see cref="Path"/>.
    /// </summary>
    public class DefaultMatch3View : ViewBase<DefaultMatch3ViewModel>
    {
        public static string Path = "Prefabs/Views/Match3/DefaultMatch3View";

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
