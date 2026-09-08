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

function copyMap<T>(source: Record<string, T>): Record<string, T> {
  const result: Record<string, T> = {};
  for (const key of Object.keys(source)) {
    result[key] = source[key];
  }
  return result;
}

function serializeParams(params: ChoiceActionParamsData): ChoiceActionParamsData {
  return {
    Strings: copyMap(params.Strings),
    Ints: copyMap(params.Ints),
    Bools: copyMap(params.Bools),
  };
}

function serializeAction(action: ChoiceActionData): ChoiceActionData {
  return {
    Type: action.Type,
    Params: serializeParams(action.Params),
  };
}

function serializeRestriction(restriction: Restriction): Restriction {
  return {
    Type: restriction.Type,
    StringValues: restriction.StringValues,
    IntValues: restriction.IntValues,
    LongValues: restriction.LongValues,
    BoolValues: restriction.BoolValues,
    CompareOptions: restriction.CompareOptions,
  };
}

function serializeVisualOptions(options: VisualOptions): VisualOptions {
  return {
    MainIcon: options.MainIcon,
    DescriptionIcon: options.DescriptionIcon,
    ParameterOverrideDescription: options.ParameterOverrideDescription,
  };
}

function serializeDiceCheck(diceCheck: ChoiceDiceCheckData): ChoiceDiceCheckData {
  return {
    DifficultyClass: diceCheck.DifficultyClass,
    DiceType: diceCheck.DiceType,
    DiceOptions: diceCheck.DiceOptions,
    DiceCheckParam: diceCheck.DiceCheckParam,
    OnCriticalSuccess: diceCheck.OnCriticalSuccess.map(serializeAction),
    OnSuccess: diceCheck.OnSuccess.map(serializeAction),
    OnFailure: diceCheck.OnFailure.map(serializeAction),
    OnCriticalFailure: diceCheck.OnCriticalFailure.map(serializeAction),
  };
}

function serializeChoice(choice: ChoiceData): ChoiceData {
  return {
    Id: choice.Id,
    Tags: [...choice.Tags],
    Type: choice.Type,
    VisualOptions: choice.VisualOptions == null ? null : serializeVisualOptions(choice.VisualOptions),
    Text: choice.Text,
    Description: choice.Description,
    AlwaysShow: choice.AlwaysShow,
    Restrictions: choice.Restrictions.map(serializeRestriction),
    DiceCheck: choice.DiceCheck == null ? null : serializeDiceCheck(choice.DiceCheck),
    Actions: choice.Actions.map(serializeAction),
  };
}

function serializeContent(content: SceneContentData): SceneContentData {
  return {
    Type: content.Type,
    Restrictions: content.Restrictions.map(serializeRestriction),
    Value: content.Value,
    Values: content.Values == null ? null : [...content.Values],
  };
}

function serializeScene(scene: SceneData): SceneData {
  return {
    Id: scene.Id,
    Tags: [...scene.Tags],
    Content: scene.Content.map(serializeContent),
    NotClearScene: scene.NotClearScene,
    Choices: scene.Choices.map(serializeChoice),
  };
}

export function toCanonicalAdventure(adventure: AdventureData): AdventureData {
  const scenes: Record<string, SceneData> = {};
  for (const sceneId of Object.keys(adventure.Scenes)) {
    scenes[sceneId] = serializeScene(adventure.Scenes[sceneId]);
  }

  return {
    Disabled: adventure.Disabled,
    Tags: [...adventure.Tags],
    IgnoredTags: [...adventure.IgnoredTags],
    IsRepeatable: adventure.IsRepeatable,
    Type: adventure.Type,
    AdventureLinks: [...adventure.AdventureLinks],
    Title: adventure.Title,
    Description: adventure.Description,
    Restrictions: adventure.Restrictions.map(serializeRestriction),
    StartScenes: [...adventure.StartScenes],
    Scenes: scenes,
  };
}

export function serializeAdventure(adventure: AdventureData): string {
  return JSON.stringify(toCanonicalAdventure(adventure), null, 2);
}
