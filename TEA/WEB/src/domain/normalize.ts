import {
  ADVENTURE_TYPE_FROM_NUMBER,
  CHOICE_ACTION_TYPE_FROM_NUMBER,
  CHOICE_TYPE_FROM_NUMBER,
  COMPARE_TYPE_FROM_NUMBER,
  DICE_TYPE_FROM_NUMBER,
  RESTRICTION_TYPE_FROM_NUMBER,
  SCENE_CONTENT_TYPE_FROM_NUMBER,
  type AdventureType,
  type ChoiceActionType,
  type ChoiceType,
  type CompareType,
  type DiceOptions,
  type DiceType,
  type RestrictionType,
  type SceneContentType,
} from "./enums";
import type {
  AdventureData,
  ChoiceActionData,
  ChoiceActionParamsData,
  ChoiceData,
  ChoiceDiceCheckData,
  Restriction,
  SceneContentData,
  SceneData,
  VisualOptions,
} from "./types";

function asRecord(value: unknown): Record<string, unknown> {
  if (value !== null && typeof value === "object" && !Array.isArray(value)) {
    return value as Record<string, unknown>;
  }
  return {};
}

function asStringArray(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.map((item) => (item == null ? "" : String(item)));
}

function asNumberArray(value: unknown): number[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.map((item) => Number(item) || 0);
}

function asBoolArray(value: unknown): boolean[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.map((item) => Boolean(item));
}

function asStringMap(value: unknown): Record<string, string> {
  const source = asRecord(value);
  const result: Record<string, string> = {};
  for (const key of Object.keys(source)) {
    const item = source[key];
    result[key] = item == null ? "" : String(item);
  }
  return result;
}

function asIntMap(value: unknown): Record<string, number> {
  const source = asRecord(value);
  const result: Record<string, number> = {};
  for (const key of Object.keys(source)) {
    result[key] = Number(source[key]) || 0;
  }
  return result;
}

function asBoolMap(value: unknown): Record<string, boolean> {
  const source = asRecord(value);
  const result: Record<string, boolean> = {};
  for (const key of Object.keys(source)) {
    result[key] = Boolean(source[key]);
  }
  return result;
}

function readEnum<T extends string>(
  value: unknown,
  fromNumber: Record<number, T>,
  fallback: T,
): T {
  if (typeof value === "string" && value.length > 0) {
    return value as T;
  }
  if (typeof value === "number" && fromNumber[value] != null) {
    return fromNumber[value];
  }
  return fallback;
}

function createDefaultDiceCheck(): ChoiceDiceCheckData {
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

function normalizeParams(value: unknown): ChoiceActionParamsData {
  const source = asRecord(value);
  return {
    Strings: asStringMap(source.Strings),
    Ints: asIntMap(source.Ints),
    Bools: asBoolMap(source.Bools),
  };
}

function normalizeAction(value: unknown): ChoiceActionData {
  const source = asRecord(value);
  return {
    Type: readEnum<ChoiceActionType>(source.Type, CHOICE_ACTION_TYPE_FROM_NUMBER, "None"),
    Params: normalizeParams(source.Params),
  };
}

function normalizeActionList(value: unknown): ChoiceActionData[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.map((item) => normalizeAction(item));
}

function normalizeRestriction(value: unknown): Restriction {
  const source = asRecord(value);
  return {
    Type: readEnum<RestrictionType>(source.Type, RESTRICTION_TYPE_FROM_NUMBER, "TimeNow"),
    StringValues: source.StringValues == null ? null : asStringArray(source.StringValues),
    IntValues: source.IntValues == null ? null : asNumberArray(source.IntValues),
    LongValues: source.LongValues == null ? null : asNumberArray(source.LongValues),
    BoolValues: source.BoolValues == null ? null : asBoolArray(source.BoolValues),
    CompareOptions: readEnum<CompareType>(source.CompareOptions, COMPARE_TYPE_FROM_NUMBER, "Equal"),
  };
}

function normalizeRestrictionList(value: unknown): Restriction[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.map((item) => normalizeRestriction(item));
}

function normalizeVisualOptions(value: unknown): VisualOptions | null {
  if (value == null) {
    return null;
  }
  const source = asRecord(value);
  return {
    MainIcon: source.MainIcon == null ? "" : String(source.MainIcon),
    DescriptionIcon: source.DescriptionIcon == null ? "" : String(source.DescriptionIcon),
    ParameterOverrideDescription:
      source.ParameterOverrideDescription == null
        ? ""
        : String(source.ParameterOverrideDescription),
  };
}

function normalizeDiceCheck(value: unknown): ChoiceDiceCheckData {
  const source = value == null ? {} : asRecord(value);
  const fallback = createDefaultDiceCheck();
  return {
    DifficultyClass:
      typeof source.DifficultyClass === "number" ? source.DifficultyClass : fallback.DifficultyClass,
    DiceType: readEnum<DiceType>(source.DiceType, DICE_TYPE_FROM_NUMBER, fallback.DiceType),
    DiceOptions:
      source.DiceOptions == null ? fallback.DiceOptions : (String(source.DiceOptions) as DiceOptions),
    DiceCheckParam: source.DiceCheckParam == null ? "" : String(source.DiceCheckParam),
    OnCriticalSuccess: normalizeActionList(source.OnCriticalSuccess),
    OnSuccess: normalizeActionList(source.OnSuccess),
    OnFailure: normalizeActionList(source.OnFailure),
    OnCriticalFailure: normalizeActionList(source.OnCriticalFailure),
  };
}

function normalizeContent(value: unknown): SceneContentData {
  const source = asRecord(value);
  return {
    Type: readEnum<SceneContentType>(source.Type, SCENE_CONTENT_TYPE_FROM_NUMBER, "Text"),
    Restrictions: normalizeRestrictionList(source.Restrictions),
    Value: source.Value == null ? null : String(source.Value),
    Values: source.Values == null ? null : asStringArray(source.Values),
  };
}

function normalizeChoice(value: unknown): ChoiceData {
  const source = asRecord(value);
  const type = readEnum<ChoiceType>(source.Type, CHOICE_TYPE_FROM_NUMBER, "Default");
  const visualOptions = normalizeVisualOptions(source.VisualOptions);
  const choice: ChoiceData = {
    Id: source.Id == null ? "" : String(source.Id),
    Tags: asStringArray(source.Tags),
    Type: type,
    VisualOptions: visualOptions,
    Text: source.Text == null ? null : String(source.Text),
    Description: source.Description == null ? null : String(source.Description),
    AlwaysShow: Boolean(source.AlwaysShow),
    Restrictions: normalizeRestrictionList(source.Restrictions),
    DiceCheck:
      type === "DiceCheck"
        ? normalizeDiceCheck(source.DiceCheck)
        : source.DiceCheck == null
          ? null
          : normalizeDiceCheck(source.DiceCheck),
    Actions: normalizeActionList(source.Actions),
  };

  return choice;
}

function normalizeScene(sceneId: string, value: unknown): SceneData {
  const source = asRecord(value);
  const content = Array.isArray(source.Content) ? source.Content.map((item) => normalizeContent(item)) : [];
  const choices = Array.isArray(source.Choices) ? source.Choices.map((item) => normalizeChoice(item)) : [];
  return {
    Id: sceneId,
    Tags: asStringArray(source.Tags),
    Content: content,
    NotClearScene: Boolean(source.NotClearScene),
    Choices: choices,
  };
}

export function normalizeAdventure(raw: unknown): AdventureData {
  const source = asRecord(raw);
  const scenesSource = asRecord(source.Scenes);
  const scenes: Record<string, SceneData> = {};

  for (const key of Object.keys(scenesSource)) {
    const sceneRaw = scenesSource[key];
    const sceneRecord = asRecord(sceneRaw);
    const sceneId = key.trim().length > 0 ? key : String(sceneRecord.Id ?? "").trim();
    if (sceneId.length === 0) {
      continue;
    }
    scenes[sceneId] = normalizeScene(sceneId, sceneRaw);
  }

  return {
    Disabled: Boolean(source.Disabled),
    Tags: asStringArray(source.Tags),
    IgnoredTags: asStringArray(source.IgnoredTags),
    IsRepeatable: Boolean(source.IsRepeatable),
    Type: readEnum<AdventureType>(source.Type, ADVENTURE_TYPE_FROM_NUMBER, "Adventure"),
    AdventureLinks: asStringArray(source.AdventureLinks),
    Title: source.Title == null ? null : String(source.Title),
    Description: source.Description == null ? null : String(source.Description),
    Restrictions: normalizeRestrictionList(source.Restrictions),
    StartScenes: asStringArray(source.StartScenes),
    Scenes: scenes,
  };
}
