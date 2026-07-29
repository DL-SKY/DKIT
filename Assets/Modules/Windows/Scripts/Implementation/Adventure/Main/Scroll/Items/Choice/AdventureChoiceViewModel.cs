using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Choice.Executors;
using System.Collections.Generic;
using Zenject;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Choice
{
    /// <summary>
    /// ViewModel for a choice panel (<see cref="ChoiceType.Default"/> and <see cref="ChoiceType.DiceCheck"/>).
    /// Default runs <see cref="ChoiceData.Actions"/> via <see cref="ChoiceActionExecutorFactory"/>.
    /// DiceCheck runtime outcome flow is not implemented yet.
    /// </summary>
    public sealed class AdventureChoiceViewModel : AdventureChoiceViewModelBase
    {
        [Inject] private readonly DiContainer _container;

        private bool _isSelecting;

        public override void Select()
        {
            if (_isSelecting || IsDisposed || Data == null)
                return;

            _isSelecting = true;

            try
            {
                if (ChoiceType == ChoiceType.DiceCheck)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[{nameof(AdventureChoiceViewModel)}] DiceCheck runtime is not implemented yet (choice id '{Data.Id}').");
                    return;
                }

                ExecuteActions(Data.Actions);
            }
            finally
            {
                _isSelecting = false;
            }
        }

        private void ExecuteActions(List<ChoiceActionData> actions)
        {
            if (actions == null || actions.Count == 0)
                return;

            var factory = _container.Instantiate<ChoiceActionExecutorFactory>();

            for (int i = 0; i < actions.Count; i++)
            {
                var actionData = actions[i];
                if (actionData == null)
                    continue;

                var executor = factory.Create(actionData);
                executor.Execute();
            }
        }
    }
}
