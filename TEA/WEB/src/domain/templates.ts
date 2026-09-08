import { Characters, ChoiceActions, Windows } from "./glossary";
import type {
  ChoiceActionData,
  ChoiceActionParamsData,
  ChoiceData,
  ChoiceDiceCheckData,
  Restriction,
  SceneContentData,
} from "./types";
import type { SceneContentType } from "./enums";

function emptyParams(): ChoiceActionParamsData {
  return { Strings: {}, Ints: {}, Bools: {} };
}

export function createDefaultRestriction(): Restriction {
  return {
    Type: "TimeNow",
    CompareOptions: "Equal",
    StringValues: [],
    IntValues: [],
    LongValues: [],
    BoolValues: [],
  };
}

export function createDefaultDiceCheck(): ChoiceDiceCheckData {
  return {
    DifficultyClass: 15,
    DiceType: "D20",
    DiceOptions: "None",
    DiceCheckParam: "",
    OnCriticalSuccess: [],
    OnSuccess: [],
    OnFailure: [],
    OnCriticalFailure: [],
  };
}

export function createEmptyVisualOptions() {
  return {
    MainIcon: "",
    DescriptionIcon: "",
    ParameterOverrideDescription: "",
  };
}

export function createSceneContent(type: SceneContentType): SceneContentData {
  const content: SceneContentData = {
    Type: type,
    Restrictions: [],
    Value: null,
    Values: null,
  };
  if (type === "RandomImage" || type === "Slideshow") {
    content.Values = [];
  } else {
    content.Value = "";
  }
  return content;
}

export const SCENE_CONTENT_CREATE_OPTIONS: { id: string; label: string; type: SceneContentType }[] = [
  { id: "content.Text", label: "Text", type: "Text" },
  { id: "content.Image", label: "Image", type: "Image" },
  { id: "content.RandomImage", label: "Random Image", type: "RandomImage" },
  { id: "content.Slideshow", label: "Slideshow", type: "Slideshow" },
  { id: "content.Splitter", label: "Splitter", type: "Splitter" },
  { id: "content.Item", label: "Item", type: "Item" },
];

export function createDefaultChoice(id: string): ChoiceData {
  return {
    Id: id,
    Tags: [],
    Type: "Default",
    VisualOptions: createEmptyVisualOptions(),
    Text: "",
    Description: "",
    AlwaysShow: false,
    Restrictions: [],
    DiceCheck: null,
    Actions: [],
  };
}

export function createDiceCheckChoice(id: string): ChoiceData {
  return {
    Id: id,
    Tags: [],
    Type: "DiceCheck",
    VisualOptions: createEmptyVisualOptions(),
    Text: "",
    Description: "",
    AlwaysShow: false,
    Restrictions: [],
    DiceCheck: createDefaultDiceCheck(),
    Actions: [],
  };
}

export const CHOICE_CREATE_OPTIONS = [
  { id: "choice.default", label: "Choice", create: createDefaultChoice },
  { id: "choice.dice_check", label: "Dice Check", create: createDiceCheckChoice },
] as const;

export function createGoToSceneAction(): ChoiceActionData {
  return {
    Type: "GoToScene",
    Params: {
      Strings: { [ChoiceActions.SCENE_ID]: "start" },
      Ints: {},
      Bools: {},
    },
  };
}

export function createGoToAdventureAction(): ChoiceActionData {
  return {
    Type: "GoToAdventure",
    Params: {
      Strings: { [ChoiceActions.ADVENTURE_ID]: "" },
      Ints: {},
      Bools: {},
    },
  };
}

export function createGoToRandomAdventureAction(): ChoiceActionData {
  return {
    Type: "GoToRandomAdventure",
    Params: emptyParams(),
  };
}

export function createGoToRandomSceneAction(): ChoiceActionData {
  return {
    Type: "GoToRandomScene",
    Params: {
      Strings: { [ChoiceActions.SCENE_ID]: "scene_a;scene_b" },
      Ints: {},
      Bools: {},
    },
  };
}

export function createOpenWindowAction(): ChoiceActionData {
  return {
    Type: "OpenWindow",
    Params: {
      Strings: { [ChoiceActions.WINDOW_ID]: Windows.CREATE_CHARACTER },
      Ints: {},
      Bools: {},
    },
  };
}

export function createSetWorldParamsAction(): ChoiceActionData {
  return {
    Type: "SetWorldParams",
    Params: {
      Strings: { "world.sample": "value" },
      Ints: {},
      Bools: {},
    },
  };
}

export function createSetAdventureParamsAction(): ChoiceActionData {
  return {
    Type: "SetAdventureParams",
    Params: {
      Strings: {},
      Ints: { "adventure.sample": 1 },
      Bools: {},
    },
  };
}

export function createSetGlobalParamsAction(): ChoiceActionData {
  return {
    Type: "SetGlobalParams",
    Params: {
      Strings: {},
      Ints: { "global.sample": 1 },
      Bools: {},
    },
  };
}

export function createSetCharacterParameterAction(): ChoiceActionData {
  return {
    Type: "SetCharacterParameter",
    Params: {
      Strings: { [ChoiceActions.PARAMETER_KEY]: Characters.EXPERIENCE },
      Ints: { [ChoiceActions.PARAMETER_VALUE]: 0 },
      Bools: {},
    },
  };
}

export function createAddCharacterParameterAction(): ChoiceActionData {
  return {
    Type: "AddCharacterParameter",
    Params: {
      Strings: { [ChoiceActions.PARAMETER_KEY]: Characters.EXPERIENCE },
      Ints: { [ChoiceActions.PARAMETER_DELTA]: 100 },
      Bools: {},
    },
  };
}

export const CHOICE_ACTION_CREATE_OPTIONS: { id: string; label: string; create: () => ChoiceActionData }[] = [
  { id: "action.goto_scene", label: "Go To Scene", create: createGoToSceneAction },
  { id: "action.goto_adventure", label: "Go To Adventure", create: createGoToAdventureAction },
  { id: "action.goto_random_adventure", label: "Go To Random Adventure", create: createGoToRandomAdventureAction },
  { id: "action.goto_random_scene", label: "Go To Random Scene", create: createGoToRandomSceneAction },
  { id: "action.open_window", label: "Open Window", create: createOpenWindowAction },
  { id: "action.set_world_params", label: "Set World Params", create: createSetWorldParamsAction },
  { id: "action.set_adventure_params", label: "Set Adventure Params", create: createSetAdventureParamsAction },
  { id: "action.set_global_params", label: "Set Global Params", create: createSetGlobalParamsAction },
  { id: "action.set_character_parameter", label: "Set Character Parameter", create: createSetCharacterParameterAction },
  { id: "action.add_character_parameter", label: "Add Character Parameter", create: createAddCharacterParameterAction },
];

export const KNOWN_WINDOW_IDS = [
  Windows.CREATE_CHARACTER,
  Windows.SELECT_CHARACTER,
  Windows.TRADE,
  Windows.PARTY,
  Windows.ADVENTURE_LIST,
] as const;

export const CHARACTER_PARAMETER_KEYS = [
  Characters.LEVEL,
  Characters.EXPERIENCE,
  Characters.BOOST_POINTS,
  Characters.STR,
  Characters.DEX,
  Characters.CON,
  Characters.INT,
  Characters.WIS,
  Characters.CHA,
  Characters.PERCEPTION,
  Characters.THIEVERY,
] as const;
