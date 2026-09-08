import type {
  AdventureType,
  ChoiceActionType,
  ChoiceType,
  CompareType,
  DiceOptions,
  DiceType,
  RestrictionType,
  SceneContentType,
} from "./enums";

export type StringMap = Record<string, string>;
export type IntMap = Record<string, number>;
export type BoolMap = Record<string, boolean>;

export type Restriction = {
  Type: RestrictionType;
  StringValues: string[] | null;
  IntValues: number[] | null;
  LongValues: number[] | null;
  BoolValues: boolean[] | null;
  CompareOptions: CompareType;
};

export type ChoiceActionParamsData = {
  Strings: StringMap;
  Ints: IntMap;
  Bools: BoolMap;
};

export type ChoiceActionData = {
  Type: ChoiceActionType;
  Params: ChoiceActionParamsData;
};

export type VisualOptions = {
  MainIcon: string;
  DescriptionIcon: string;
  ParameterOverrideDescription: string;
};

export type ChoiceDiceCheckData = {
  DifficultyClass: number;
  DiceType: DiceType;
  DiceOptions: DiceOptions;
  DiceCheckParam: string;
  OnCriticalSuccess: ChoiceActionData[];
  OnSuccess: ChoiceActionData[];
  OnFailure: ChoiceActionData[];
  OnCriticalFailure: ChoiceActionData[];
};

export type ChoiceData = {
  Id: string;
  Tags: string[];
  Type: ChoiceType;
  VisualOptions: VisualOptions | null;
  Text: string | null;
  Description: string | null;
  AlwaysShow: boolean;
  Restrictions: Restriction[];
  DiceCheck: ChoiceDiceCheckData | null;
  Actions: ChoiceActionData[];
};

export type SceneContentData = {
  Type: SceneContentType;
  Restrictions: Restriction[];
  Value: string | null;
  Values: string[] | null;
};

export type SceneData = {
  Id: string;
  Tags: string[];
  Content: SceneContentData[];
  NotClearScene: boolean;
  Choices: ChoiceData[];
};

export type AdventureData = {
  Disabled: boolean;
  Tags: string[];
  IgnoredTags: string[];
  IsRepeatable: boolean;
  Type: AdventureType;
  AdventureLinks: string[];
  Title: string | null;
  Description: string | null;
  Restrictions: Restriction[];
  StartScenes: string[];
  Scenes: Record<string, SceneData>;
};
