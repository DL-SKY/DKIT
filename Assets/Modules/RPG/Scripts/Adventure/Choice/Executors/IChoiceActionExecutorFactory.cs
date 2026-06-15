namespace Modules.RPG.Scripts.Adventure.Choice.Executors
{
    public interface IChoiceActionExecutorFactory
    {
        IChoiceActionExecutor Create(ChoiceActionData actionData);
    }
}
