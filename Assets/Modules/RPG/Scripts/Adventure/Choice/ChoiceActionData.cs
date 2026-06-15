using Modules.RPG.Scripts.Adventure.Choice.Actions;
using System.Collections.Generic;

namespace Modules.RPG.Scripts.Adventure.Choice
{
    public class ChoiceActionData
    {
        public ChoiceActionType Type;

        public ChoiceActionParamsData Params;
    }

    public class ChoiceActionParamsData
    {
        public Dictionary<string, string> Strings;
        public Dictionary<string, int> Ints;
        public Dictionary<string, bool> Bools;
        public Dictionary<string, float> Floats;
    }

    // JSON example: Change character HP by stat key
    // {
    //   "Type": "ModifyVariable",
    //   "Params": {
    //     "Strings": { "key": "char.hp" },
    //     "Ints": { "delta": -5 }
    //   }
    // }
    //
    // JSON example: Skill check and branching
    // {
    //   "Type": "SkillCheck",
    //   "Params": {
    //     "Strings": {
    //       "skill": "char.skill.stealth",
    //       "onSuccessSceneId": "SCENE_STEALTH_OK",
    //       "onFailSceneId": "SCENE_STEALTH_FAIL",
    //       "advantageMode": "none"
    //     },
    //     "Ints": { "dc": 12 }
    //   }
    // }
    //
    // JSON example: Set world flag
    // {
    //   "Type": "SetFlag",
    //   "Params": {
    //     "Strings": { "key": "world.tavern_unlocked" },
    //     "Bools": { "value": true }
    //   }
    // }
    //
    // Params interpretation example (runtime pseudo-code):
    // switch (action.Type)
    // {
    //     case ChoiceActionType.ModifyVariable:
    //     // {
    //     //     var key = action.Params.Strings["key"];
    //     //     var delta = action.Params.Ints["delta"];
    //     //     state.Adventure.IntVariables[key] += delta;
    //     //     break;
    //     // }
    // }
}
