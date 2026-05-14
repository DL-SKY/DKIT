using Modules.Windows.Scripts.Base;
using Modules.Windows.Scripts.Settings;

namespace Modules.Windows.Scripts.Implementation.Match3
{
    /// <summary>
    /// Заготовка VM для экрана Match3: сюда — состояние, подписки на сервисы, команды для View.
    /// </summary>
    public class DefaultMatch3ViewModel : ViewModelBase
    {
        public override void Dispose()
        {
            // Отписки от внешних источников, если добавите вне Subscribe внутри VM.
        }
    }
}
