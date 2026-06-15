namespace Modules.RPG.Scripts.Adventure.Choice.Actions
{
    public enum ChoiceActionType
    {
        None = 0,

        GoToNode = 100,
        SetFlag = 110,
        ModifyVariable = 120,

        SkillCheck = 200,
        StartCombat = 300,

        ApplyDamage = 400,
        Heal = 410,
        GrantItem = 500,
    }
}
