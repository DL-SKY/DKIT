import { ChoiceActions } from "./glossary";
import { getGraphSceneTargets, isGraphSceneEdgeAction } from "./sceneTargets";
import { createDefaultDiceCheck } from "./templates";
import type { ChoiceActionType } from "./enums";
import type { AdventureData, ChoiceActionData, ChoiceData } from "./types";

export type ValidationIssueLocation =
  | { scope: "adventure" }
  | { scope: "scene"; sceneId: string }
  | { scope: "content"; sceneId: string; contentIndex: number }
  | { scope: "choice"; sceneId: string; choiceIndex: number }
  | { scope: "action"; sceneId: string; choiceIndex: number; actionIndex: number };

export type ValidationIssue = {
  message: string;
  location?: ValidationIssueLocation;
  applyFix?: (adventure: AdventureData) => void;
};

export type ValidationIssueFixFilter = "all" | "fixable" | "unfixable";

const UPPER_SNAKE_TAG_REGEX = /^[A-Z0-9]+(?:_[A-Z0-9]+)*$/;
const KEY_WITH_UNDERSCORE_REGEX = /^[A-Za-z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)+$/;
const LOWER_SNAKE_ID_REGEX = /^[a-z0-9]+(?:_[a-z0-9]+)*$/;
const CAMEL_OR_PASCAL_ID_REGEX = /^[A-Za-z][A-Za-z0-9]*$/;
const LOC_PREFIX = "loc:";
const MAX_FIX_PASSES = 64;

type IdentifierStyle = "Unknown" | "LowerSnake" | "CamelOrPascal" | "Other";

type ChoiceActionValidationContract = {
  requiredStringKeys: string[];
  requiredIntKeys: string[];
  requiredBoolKeys: string[];
  allowAnyStringKeys: boolean;
  allowAnyIntKeys: boolean;
  allowAnyBoolKeys: boolean;
  requireAnyParam: boolean;
};

type TagScope =
  | { kind: "adventure"; field: "Tags" | "IgnoredTags" }
  | { kind: "scene"; sceneId: string }
  | { kind: "choice"; sceneId: string; choiceIndex: number };

function isSingleStringKeyContract(contract: ChoiceActionValidationContract): boolean {
  return (
    contract.requiredStringKeys.length === 1 &&
    contract.requiredIntKeys.length === 0 &&
    contract.requiredBoolKeys.length === 0
  );
}

function getChoiceActionContract(type: ChoiceActionType): ChoiceActionValidationContract | null {
  switch (type) {
    case "GoToScene":
      return createContract({ requiredStringKeys: [ChoiceActions.SCENE_ID] });
    case "GoToAdventure":
      return createContract({ requiredStringKeys: [ChoiceActions.ADVENTURE_ID] });
    case "GoToRandomAdventure":
      return createContract({});
    case "GoToRandomScene":
      return createContract({ requiredStringKeys: [ChoiceActions.SCENE_ID] });
    case "OpenWindow":
      return createContract({ requiredStringKeys: [ChoiceActions.WINDOW_ID] });
    case "SetWorldParams":
    case "SetAdventureParams":
    case "SetGlobalParams":
      return createContract({
        allowAnyStringKeys: true,
        allowAnyIntKeys: true,
        allowAnyBoolKeys: true,
        requireAnyParam: true,
      });
    case "SetCharacterParameter":
      return createContract({
        requiredStringKeys: [ChoiceActions.PARAMETER_KEY],
        requiredIntKeys: [ChoiceActions.PARAMETER_VALUE],
      });
    case "AddCharacterParameter":
      return createContract({
        requiredStringKeys: [ChoiceActions.PARAMETER_KEY],
        requiredIntKeys: [ChoiceActions.PARAMETER_DELTA],
      });
    default:
      return null;
  }
}

function createContract(
  partial: Partial<ChoiceActionValidationContract>,
): ChoiceActionValidationContract {
  return {
    requiredStringKeys: [],
    requiredIntKeys: [],
    requiredBoolKeys: [],
    allowAnyStringKeys: false,
    allowAnyIntKeys: false,
    allowAnyBoolKeys: false,
    requireAnyParam: false,
    ...partial,
  };
}

function looksLikeLocalizationKey(value: string): boolean {
  const trimmed = value.trim();
  if (trimmed.length === 0) {
    return false;
  }
  if (trimmed.toLowerCase().startsWith(LOC_PREFIX)) {
    return true;
  }
  if (trimmed.includes(" ")) {
    return false;
  }
  return KEY_WITH_UNDERSCORE_REGEX.test(trimmed);
}

function validateLocalizedTextField(
  issues: ValidationIssue[],
  fieldPath: string,
  value: string | null,
  location?: ValidationIssueLocation,
): void {
  if (value == null || value.length === 0) {
    return;
  }
  if (!looksLikeLocalizationKey(value)) {
    return;
  }
  issues.push({
    message: `${fieldPath} looks like a localization key ('${value}'). TEA adventure JSON should store user-facing text unless explicitly requested otherwise.`,
    location,
  });
}

function normalizeTagToUpperSnake(value: string): string {
  let builder = "";
  for (const char of value) {
    if (/[a-zA-Z0-9]/.test(char)) {
      builder += char.toUpperCase();
    } else {
      builder += "_";
    }
  }
  return builder.replace(/_{2,}/g, "_").replace(/^_+|_+$/g, "");
}

function getTagList(adventure: AdventureData, scope: TagScope): string[] | null {
  if (scope.kind === "adventure") {
    return adventure[scope.field];
  }
  const scene = adventure.Scenes[scope.sceneId];
  if (scene == null) {
    return null;
  }
  if (scope.kind === "scene") {
    return scene.Tags;
  }
  return scene.Choices[scope.choiceIndex]?.Tags ?? null;
}

function getChoice(adventure: AdventureData, sceneId: string, choiceIndex: number): ChoiceData | null {
  return adventure.Scenes[sceneId]?.Choices[choiceIndex] ?? null;
}

function tagLocation(scope: TagScope): ValidationIssueLocation {
  if (scope.kind === "adventure") {
    return { scope: "adventure" };
  }
  if (scope.kind === "scene") {
    return { scope: "scene", sceneId: scope.sceneId };
  }
  return { scope: "choice", sceneId: scope.sceneId, choiceIndex: scope.choiceIndex };
}

function sceneLocation(sceneId: string): ValidationIssueLocation {
  return { scope: "scene", sceneId };
}

function contentLocation(sceneId: string, contentIndex: number): ValidationIssueLocation {
  return { scope: "content", sceneId, contentIndex };
}

function choiceLocation(sceneId: string, choiceIndex: number): ValidationIssueLocation {
  return { scope: "choice", sceneId, choiceIndex };
}

function actionLocation(
  sceneId: string,
  choiceIndex: number,
  actionIndex: number,
): ValidationIssueLocation {
  return { scope: "action", sceneId, choiceIndex, actionIndex };
}

function getAction(
  adventure: AdventureData,
  sceneId: string,
  choiceIndex: number,
  actionIndex: number,
): ChoiceActionData | null {
  return getChoice(adventure, sceneId, choiceIndex)?.Actions[actionIndex] ?? null;
}

function validateTagsList(issues: ValidationIssue[], tags: string[] | null, scope: TagScope, scopeLabel: string): void {
  if (tags == null) {
    return;
  }

  for (let i = 0; i < tags.length; i += 1) {
    const tag = tags[i];
    const index = i;
    if (tag == null || tag.trim().length === 0) {
      issues.push({
        message: `${scopeLabel} contains an empty tag at index ${index}.`,
        location: tagLocation(scope),
        applyFix: (adventure) => {
          const list = getTagList(adventure, scope);
          if (list != null && index < list.length) {
            list.splice(index, 1);
          }
        },
      });
      continue;
    }
    if (UPPER_SNAKE_TAG_REGEX.test(tag)) {
      continue;
    }
    const normalizedTag = normalizeTagToUpperSnake(tag);
    if (normalizedTag.length === 0) {
      issues.push({
        message: `${scopeLabel} tag '${tag}' cannot be normalized to UPPER_SNAKE_CASE.`,
        location: tagLocation(scope),
      });
      continue;
    }
    issues.push({
      message: `${scopeLabel} tag '${tag}' must be UPPER_SNAKE_CASE.`,
      location: tagLocation(scope),
      applyFix: (adventure) => {
        const list = getTagList(adventure, scope);
        if (list != null && index < list.length) {
          list[index] = normalizedTag;
        }
      },
    });
  }
}

function detectIdentifierStyle(value: string | null | undefined): IdentifierStyle {
  if (value == null || value.trim().length === 0) {
    return "Unknown";
  }
  if (LOWER_SNAKE_ID_REGEX.test(value)) {
    return "LowerSnake";
  }
  if (CAMEL_OR_PASCAL_ID_REGEX.test(value)) {
    return "CamelOrPascal";
  }
  return "Other";
}

function validateIdentifierStyleConsistency(issues: ValidationIssue[], adventure: AdventureData): void {
  const sceneStyles = new Set<IdentifierStyle>();
  const choiceStyles = new Set<IdentifierStyle>();

  for (const sceneId of Object.keys(adventure.Scenes)) {
    const sceneStyle = detectIdentifierStyle(sceneId);
    if (sceneStyle === "Other") {
      issues.push({
        message: `Scene id '${sceneId}' has an unsupported style. Use either lower_snake_case or CamelCase/PascalCase.`,
        location: sceneLocation(sceneId),
      });
    } else if (sceneStyle !== "Unknown") {
      sceneStyles.add(sceneStyle);
    }

    const scene = adventure.Scenes[sceneId];
    if (scene?.Choices == null) {
      continue;
    }

    for (let choiceIndex = 0; choiceIndex < scene.Choices.length; choiceIndex += 1) {
      const choice = scene.Choices[choiceIndex];
      const choiceStyle = detectIdentifierStyle(choice?.Id);
      if (choiceStyle === "Other") {
        issues.push({
          message: `Choice id '${choice.Id}' in scene '${sceneId}' has an unsupported style. Use either lower_snake_case or CamelCase/PascalCase.`,
          location: choiceLocation(sceneId, choiceIndex),
        });
      } else if (choiceStyle !== "Unknown") {
        choiceStyles.add(choiceStyle);
      }
    }
  }

  if (sceneStyles.size > 1) {
    issues.push({
      message: "Scene ids use mixed styles. Keep one style consistently across adventure scenes.",
      location: { scope: "adventure" },
    });
  }
  if (choiceStyles.size > 1) {
    issues.push({
      message: "Choice ids use mixed styles. Keep one style consistently across adventure choices.",
      location: { scope: "adventure" },
    });
  }
}

function findFixCandidateKey(strings: Record<string, string>, expectedKey: string): string {
  const keys = Object.keys(strings);
  if (keys.length === 1) {
    return keys[0] ?? "";
  }
  if (expectedKey === ChoiceActions.SCENE_ID && strings.sceneId != null) {
    return "sceneId";
  }
  return "";
}

function validateStringKeys(
  issues: ValidationIssue[],
  action: ChoiceActionData,
  contract: ChoiceActionValidationContract,
  sceneId: string,
  choiceId: string,
  choiceIndex: number,
  actionIndex: number,
): void {
  const strings = action.Params.Strings;
  if (contract.allowAnyStringKeys) {
    return;
  }

  const expected = new Set(contract.requiredStringKeys);
  for (const key of contract.requiredStringKeys) {
    const value = strings[key];
    if (value != null && value.trim().length > 0) {
      continue;
    }

    if (!isSingleStringKeyContract(contract)) {
      issues.push({
        message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} is missing required Strings key '${key}' for type '${action.Type}'.`,
        location: actionLocation(sceneId, choiceIndex, actionIndex),
      });
      continue;
    }

    const candidateKey = findFixCandidateKey(strings, key);
    if (candidateKey.length === 0) {
      issues.push({
        message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} is missing required Strings key '${key}' for type '${action.Type}'.`,
        location: actionLocation(sceneId, choiceIndex, actionIndex),
      });
      continue;
    }

    issues.push({
      message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} uses wrong Strings key '${candidateKey}'. Expected '${key}' for type '${action.Type}'.`,
      location: actionLocation(sceneId, choiceIndex, actionIndex),
      applyFix: (adventure) => {
        const target = getAction(adventure, sceneId, choiceIndex, actionIndex)?.Params.Strings;
        if (target == null || target[candidateKey] == null) {
          return;
        }
        target[key] = target[candidateKey];
        if (candidateKey !== key) {
          delete target[candidateKey];
        }
      },
    });
  }

  for (const key of Object.keys(strings)) {
    if (expected.has(key)) {
      continue;
    }
    if (isSingleStringKeyContract(contract) && contract.requiredStringKeys.length === 1) {
      const expectedKey = contract.requiredStringKeys[0];
      issues.push({
        message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} contains unexpected Strings key '${key}' for type '${action.Type}'.`,
        location: actionLocation(sceneId, choiceIndex, actionIndex),
        applyFix: (adventure) => {
          const target = getAction(adventure, sceneId, choiceIndex, actionIndex)?.Params.Strings;
          if (target == null || target[key] == null) {
            return;
          }
          if (target[expectedKey] == null) {
            target[expectedKey] = target[key];
          }
          delete target[key];
        },
      });
      continue;
    }
    issues.push({
      message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} contains unexpected Strings key '${key}' for type '${action.Type}'.`,
      location: actionLocation(sceneId, choiceIndex, actionIndex),
    });
  }
}

function validateUnexpectedKeys(
  issues: ValidationIssue[],
  dictionary: Record<string, unknown>,
  expectedKeys: string[],
  allowAnyKeys: boolean,
  groupName: string,
  sceneId: string,
  choiceId: string,
  choiceIndex: number,
  actionIndex: number,
): void {
  if (allowAnyKeys) {
    return;
  }
  const expected = new Set(expectedKeys);
  for (const key of Object.keys(dictionary)) {
    if (expected.has(key)) {
      continue;
    }
    issues.push({
      message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} contains unexpected ${groupName} key '${key}'.`,
      location: actionLocation(sceneId, choiceIndex, actionIndex),
    });
  }
}

function validateActionParamsContract(
  issues: ValidationIssue[],
  action: ChoiceActionData,
  sceneId: string,
  choiceId: string,
  choiceIndex: number,
  actionIndex: number,
): void {
  const contract = getChoiceActionContract(action.Type);
  if (contract == null) {
    issues.push({
      message:
        `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} has type '${action.Type}' without TEA validation contract. ` +
        "Add contract validation for this ChoiceActionType and Glossary keys.",
      location: actionLocation(sceneId, choiceIndex, actionIndex),
    });
    return;
  }

  if (
    contract.requireAnyParam &&
    Object.keys(action.Params.Strings).length === 0 &&
    Object.keys(action.Params.Ints).length === 0 &&
    Object.keys(action.Params.Bools).length === 0
  ) {
    issues.push({
      message: `Choice '${choiceId}' in scene '${sceneId}' action #${actionIndex} of type '${action.Type}' must contain at least one param.`,
      location: actionLocation(sceneId, choiceIndex, actionIndex),
    });
  }

  validateStringKeys(issues, action, contract, sceneId, choiceId, choiceIndex, actionIndex);
  validateUnexpectedKeys(
    issues,
    action.Params.Ints,
    contract.requiredIntKeys,
    contract.allowAnyIntKeys,
    "Ints",
    sceneId,
    choiceId,
    choiceIndex,
    actionIndex,
  );
  validateUnexpectedKeys(
    issues,
    action.Params.Bools,
    contract.requiredBoolKeys,
    contract.allowAnyBoolKeys,
    "Bools",
    sceneId,
    choiceId,
    choiceIndex,
    actionIndex,
  );
}

export function canFixIssue(issue: ValidationIssue): boolean {
  return issue.applyFix != null;
}

export function canNavigateToIssue(issue: ValidationIssue): boolean {
  return issue.location != null && issue.location.scope !== "adventure";
}

export function filterValidationIssues(
  issues: ValidationIssue[],
  query: string,
  fixFilter: ValidationIssueFixFilter,
): ValidationIssue[] {
  const needle = query.trim().toLowerCase();
  return issues.filter((item) => {
    if (fixFilter === "fixable" && item.applyFix == null) {
      return false;
    }
    if (fixFilter === "unfixable" && item.applyFix != null) {
      return false;
    }
    if (needle.length > 0 && !item.message.toLowerCase().includes(needle)) {
      return false;
    }
    return true;
  });
}

export function applyValidationFix(adventure: AdventureData, issue: ValidationIssue): AdventureData {
  const next = structuredClone(adventure);
  issue.applyFix?.(next);
  return next;
}

export function applyAllValidationFixes(adventure: AdventureData): AdventureData {
  const next = structuredClone(adventure);
  for (let pass = 0; pass < MAX_FIX_PASSES; pass += 1) {
    const issues = validateAdventure(next);
    const issue = issues.find((item) => item.applyFix != null);
    if (issue?.applyFix == null) {
      break;
    }
    issue.applyFix(next);
  }
  return next;
}

export function validateAdventure(adventure: AdventureData | null): ValidationIssue[] {
  const issues: ValidationIssue[] = [];
  const adventureLocation: ValidationIssueLocation = { scope: "adventure" };
  if (adventure == null) {
    issues.push({ message: "Adventure is null.", location: adventureLocation });
    return issues;
  }

  const sceneIds = Object.keys(adventure.Scenes);
  if (sceneIds.length === 0) {
    issues.push({ message: "Adventure must contain at least one scene.", location: adventureLocation });
  }
  if (adventure.StartScenes.length === 0) {
    issues.push({
      message: "Adventure must contain at least one start scene.",
      location: adventureLocation,
    });
  }

  validateTagsList(issues, adventure.Tags, { kind: "adventure", field: "Tags" }, "Adventure.Tags");
  validateTagsList(
    issues,
    adventure.IgnoredTags,
    { kind: "adventure", field: "IgnoredTags" },
    "Adventure.IgnoredTags",
  );
  validateLocalizedTextField(issues, "Adventure.Title", adventure.Title, adventureLocation);
  validateLocalizedTextField(issues, "Adventure.Description", adventure.Description, adventureLocation);

  for (const startSceneId of adventure.StartScenes) {
    if (startSceneId == null || startSceneId.trim().length === 0) {
      issues.push({ message: "Start scene id cannot be empty.", location: adventureLocation });
      continue;
    }
    if (adventure.Scenes[startSceneId] == null) {
      issues.push({
        message: `Start scene '${startSceneId}' does not exist in Scenes.`,
        location: adventureLocation,
      });
    }
  }

  for (const sceneId of sceneIds) {
    const sceneData = adventure.Scenes[sceneId];
    if (sceneId.trim().length === 0) {
      issues.push({ message: "Scene dictionary contains empty scene id.", location: adventureLocation });
    }
    if (sceneData == null) {
      issues.push({ message: `Scene '${sceneId}' is null.`, location: adventureLocation });
      continue;
    }
    if (sceneData.Id !== sceneId) {
      issues.push({
        message: `Scene key '${sceneId}' does not match SceneData.Id '${sceneData.Id}'.`,
        location: sceneLocation(sceneId),
      });
    }

    validateTagsList(issues, sceneData.Tags, { kind: "scene", sceneId }, `Scene '${sceneId}'.Tags`);

    for (let contentIndex = 0; contentIndex < sceneData.Content.length; contentIndex += 1) {
      const contentData = sceneData.Content[contentIndex];
      if (contentData == null) {
        continue;
      }
      validateLocalizedTextField(
        issues,
        `Scene '${sceneId}' content #${contentIndex} Value`,
        contentData.Value,
        contentLocation(sceneId, contentIndex),
      );
    }

    const choiceIds = new Set<string>();
    for (let choiceIndex = 0; choiceIndex < sceneData.Choices.length; choiceIndex += 1) {
      const choice = sceneData.Choices[choiceIndex];
      if (choice == null) {
        issues.push({
          message: `Scene '${sceneId}' contains null choice.`,
          location: sceneLocation(sceneId),
        });
        continue;
      }

      const choiceId = choice.Id.trim().length === 0 ? `choice_${choiceIndex}` : choice.Id;
      if (choice.Id.trim().length > 0) {
        if (choiceIds.has(choice.Id)) {
          issues.push({
            message: `Scene '${sceneId}' contains duplicated choice id '${choice.Id}'.`,
            location: choiceLocation(sceneId, choiceIndex),
          });
        } else {
          choiceIds.add(choice.Id);
        }
      }

      validateTagsList(
        issues,
        choice.Tags,
        { kind: "choice", sceneId, choiceIndex },
        `Choice '${choiceId}' in scene '${sceneId}'.Tags`,
      );
      validateLocalizedTextField(
        issues,
        `Choice '${choiceId}' in scene '${sceneId}' Text`,
        choice.Text,
        choiceLocation(sceneId, choiceIndex),
      );
      validateLocalizedTextField(
        issues,
        `Choice '${choiceId}' in scene '${sceneId}' Description`,
        choice.Description,
        choiceLocation(sceneId, choiceIndex),
      );

      if (choice.Type === "Default") {
        if (choice.DiceCheck != null) {
          issues.push({
            message: `Choice '${choiceId}' in scene '${sceneId}' has type 'Default' but DiceCheck block is set. DiceCheck must be null for Default.`,
            location: choiceLocation(sceneId, choiceIndex),
            applyFix: (target) => {
              const nextChoice = getChoice(target, sceneId, choiceIndex);
              if (nextChoice != null) {
                nextChoice.DiceCheck = null;
              }
            },
          });
        }
      } else if (choice.Type === "DiceCheck") {
        if (choice.DiceCheck == null) {
          issues.push({
            message: `Choice '${choiceId}' in scene '${sceneId}' has type 'DiceCheck' but DiceCheck block is missing.`,
            location: choiceLocation(sceneId, choiceIndex),
            applyFix: (target) => {
              const nextChoice = getChoice(target, sceneId, choiceIndex);
              if (nextChoice != null) {
                nextChoice.DiceCheck = createDefaultDiceCheck();
              }
            },
          });
        } else if (choice.DiceCheck.DifficultyClass < 0) {
          issues.push({
            message: `Choice '${choiceId}' in scene '${sceneId}' has negative DiceCheck.DifficultyClass.`,
            location: choiceLocation(sceneId, choiceIndex),
            applyFix: (target) => {
              const diceCheck = getChoice(target, sceneId, choiceIndex)?.DiceCheck;
              if (diceCheck != null) {
                diceCheck.DifficultyClass = 0;
              }
            },
          });
        }

        if (choice.Actions.length > 0) {
          issues.push({
            message: `Choice '${choiceId}' in scene '${sceneId}' has type 'DiceCheck' but contains ${choice.Actions.length} item(s) in Actions. For DiceCheck, Actions must be null or empty.`,
            location: choiceLocation(sceneId, choiceIndex),
            applyFix: (target) => {
              const nextChoice = getChoice(target, sceneId, choiceIndex);
              if (nextChoice != null) {
                nextChoice.Actions.length = 0;
              }
            },
          });
        }
      }

      if (choice.Type !== "Default") {
        continue;
      }

      for (let actionIndex = 0; actionIndex < choice.Actions.length; actionIndex += 1) {
        const action = choice.Actions[actionIndex];
        if (action == null) {
          issues.push({
            message: `Scene '${sceneId}' contains null action in choice '${choiceId}'.`,
            location: choiceLocation(sceneId, choiceIndex),
          });
          continue;
        }

        validateActionParamsContract(issues, action, sceneId, choiceId, choiceIndex, actionIndex);
        if (!isGraphSceneEdgeAction(action.Type)) {
          continue;
        }

        const targetSceneIds = getGraphSceneTargets(action);
        if (targetSceneIds.length === 0) {
          issues.push({
            message: `Choice '${choiceId}' in scene '${sceneId}' has scene transition action with empty SceneId.`,
            location: actionLocation(sceneId, choiceIndex, actionIndex),
          });
          continue;
        }

        for (const targetSceneId of targetSceneIds) {
          if (adventure.Scenes[targetSceneId] == null) {
            issues.push({
              message: `Choice '${choiceId}' in scene '${sceneId}' points to missing scene '${targetSceneId}'.`,
              location: actionLocation(sceneId, choiceIndex, actionIndex),
            });
          }
        }
      }
    }
  }

  validateIdentifierStyleConsistency(issues, adventure);
  return issues;
}
