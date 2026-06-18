using Modules.Windows.Scripts.Base;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main
{
    /// <summary>
    /// Заготовка VM для главного экрана Adventure: сюда — состояние, подписки на сервисы, команды для View.
    /// </summary>
    public class AdventureMainViewModel : ViewModelBase
    {
        public void Init(string scenarioName)
        {

        }

        public override void Dispose()
        {
            // Отписки от внешних источников, если добавите вне Subscribe внутри VM.
        }
    }
}
