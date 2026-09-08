export const ADVENTURE_TYPES = ["Adventure", "Chapter", "Location"] as const;
export type AdventureType = (typeof ADVENTURE_TYPES)[number];

export const ADVENTURE_TYPE_FROM_NUMBER: Record<number, AdventureType> = {
  0: "Adventure",
  1: "Chapter",
  10: "Location",
};

export const SCENE_CONTENT_TYPES = [
  "Text",
  "Image",
  "RandomImage",
  "Slideshow",
  "Splitter",
  "Item",
] as const;
export type SceneContentType = (typeof SCENE_CONTENT_TYPES)[number];

export const SCENE_CONTENT_TYPE_FROM_NUMBER: Record<number, SceneContentType> = {
  0: "Text",
  10: "Image",
  11: "RandomImage",
  12: "Slideshow",
  20: "Splitter",
  30: "Item",
};

export const CHOICE_TYPES = ["Default", "DiceCheck"] as const;
export type ChoiceType = (typeof CHOICE_TYPES)[number];

export const CHOICE_TYPE_FROM_NUMBER: Record<number, ChoiceType> = {
  0: "Default",
  1: "DiceCheck",
};

export const CHOICE_ACTION_TYPES = [
  "None",
  "GoToScene",
  "SetWorldParams",
  "SetAdventureParams",
  "SetGlobalParams",
  "GoToAdventure",
  "OpenWindow",
  "GoToRandomAdventure",
  "GoToRandomScene",
  "SetCharacterParameter",
  "AddCharacterParameter",
] as const;
export type ChoiceActionType = (typeof CHOICE_ACTION_TYPES)[number];

export const CHOICE_ACTION_TYPE_FROM_NUMBER: Record<number, ChoiceActionType> = {
  0: "None",
  1: "GoToScene",
  2: "SetWorldParams",
  3: "SetAdventureParams",
  4: "SetGlobalParams",
  5: "GoToAdventure",
  6: "OpenWindow",
  7: "GoToRandomAdventure",
  8: "GoToRandomScene",
  9: "SetCharacterParameter",
  10: "AddCharacterParameter",
  100: "GoToScene",
};

export const RESTRICTION_TYPES = [
  "TimeNow",
  "WorldParams",
  "AdventureParams",
  "GlobalParams",
  "ActivePartyCount",
  "CharacterParams",
] as const;
export type RestrictionType = (typeof RESTRICTION_TYPES)[number];

export const RESTRICTION_TYPE_FROM_NUMBER: Record<number, RestrictionType> = {
  0: "TimeNow",
  1: "WorldParams",
  2: "AdventureParams",
  3: "GlobalParams",
  4: "ActivePartyCount",
  5: "CharacterParams",
};

export const COMPARE_TYPES = [
  "Equal",
  "NotEqual",
  "More",
  "MoreEqual",
  "Less",
  "LessEqual",
] as const;
export type CompareType = (typeof COMPARE_TYPES)[number];

export const COMPARE_TYPE_FROM_NUMBER: Record<number, CompareType> = {
  0: "Equal",
  1: "NotEqual",
  2: "More",
  3: "MoreEqual",
  4: "Less",
  5: "LessEqual",
};

export const DICE_TYPES = ["D2", "D3", "D4", "D6", "D8", "D10", "D12", "D20", "D100"] as const;
export type DiceType = (typeof DICE_TYPES)[number];

export const DICE_TYPE_FROM_NUMBER: Record<number, DiceType> = {
  2: "D2",
  3: "D3",
  4: "D4",
  6: "D6",
  8: "D8",
  10: "D10",
  12: "D12",
  20: "D20",
  100: "D100",
};

export const DICE_OPTIONS = ["None", "Advantage", "Disadvantage", "Exploding", "Hidden"] as const;
export type DiceOptionFlag = Exclude<(typeof DICE_OPTIONS)[number], "None">;
export type DiceOptions = "None" | DiceOptionFlag | `${DiceOptionFlag}, ${string}`;
