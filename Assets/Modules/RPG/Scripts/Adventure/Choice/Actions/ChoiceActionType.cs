namespace Modules.RPG.Scripts.Adventure.Choice.Actions
{
    public enum ChoiceActionType
    {
        None = 0,

        GoToScene = 1,
        SetWorldParams = 2,
        SetAdventureParams = 3,
        SetGlobalParams = 4,
        GoToAdventure = 5,
        OpenWindow = 6,


        //TODO: rework
        //GoToScene = 100,
        //SetFlag = 110,
        //ModifyVariable = 120,

        //SkillCheck = 200,
        //StartCombat = 300,

        //ApplyDamage = 400,
        //Heal = 410,
        //GrantItem = 500,
    }
}
