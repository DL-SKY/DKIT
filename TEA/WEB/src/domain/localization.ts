import type { AdventureData, ChoiceData, SceneData } from "./types";

const LOC_PREFIX = "loc:";
const MAX_TOKEN_LENGTH = 20;
const KEY_REGEX = /^[A-Za-z][A-Za-z0-9]*(?:_[A-Za-z0-9]+)*$/;
const ARG_PLACEHOLDER_REGEX = /\{(\d+)\}/g;
const CAMEL_CASE_SPLIT_REGEX = /([a-z0-9])([A-Z])/g;
const NON_ALNUM_REGEX = /[^A-Za-z0-9]+/g;
const MULTI_UNDERSCORE_REGEX = /_{2,}/g;

export type LocalizationExportEntry = {
  key: string;
  text: string;
};

export type LocalizationFieldChange = {
  path: string;
  originalText: string;
  key: string;
  kind: "generated" | "reused";
};

export type LocalizationGenerationResult = {
  adventure: AdventureData;
  adventurePrefix: string;
  exportEntries: LocalizationExportEntry[];
  fieldChanges: LocalizationFieldChange[];
  updatedFieldsCount: number;
  generatedKeysCount: number;
  reusedKeysCount: number;
};

type GenerationContext = {
  textToGeneratedKey: Map<string, string>;
  usedKeys: Set<string>;
  exportEntries: LocalizationExportEntry[];
  fieldChanges: LocalizationFieldChange[];
  updatedFieldsCount: number;
  generatedKeysCount: number;
  reusedKeysCount: number;
};

export function generateLocalizationKeys(
  adventure: AdventureData,
  selectedFileName: string,
): LocalizationGenerationResult {
  const next = structuredClone(adventure) as AdventureData;
  const usedKeys = collectAlreadyUsedKeys(next);
  const context: GenerationContext = {
    textToGeneratedKey: new Map(),
    usedKeys,
    exportEntries: [],
    fieldChanges: [],
    updatedFieldsCount: 0,
    generatedKeysCount: 0,
    reusedKeysCount: 0,
  };
  const adventurePrefix = buildAdventurePrefix(selectedFileName);

  processField(next.Title, `${adventurePrefix}_ADV_TITLE`, "Adventure.Title", context, (value) => {
    next.Title = value;
  });
  processField(
    next.Description,
    `${adventurePrefix}_ADV_DESCR`,
    "Adventure.Description",
    context,
    (value) => {
      next.Description = value;
    },
  );

  const orderedSceneIds = Object.keys(next.Scenes).sort(compareOrdinal);
  for (const sceneId of orderedSceneIds) {
    const sceneData = next.Scenes[sceneId];
    if (sceneData == null) {
      continue;
    }
    processScene(sceneData, adventurePrefix, context);
  }

  return {
    adventure: next,
    adventurePrefix,
    exportEntries: context.exportEntries,
    fieldChanges: context.fieldChanges,
    updatedFieldsCount: context.updatedFieldsCount,
    generatedKeysCount: context.generatedKeysCount,
    reusedKeysCount: context.reusedKeysCount,
  };
}

export function formatLocalizationTsv(entries: LocalizationExportEntry[]): string {
  const lines: string[] = [];
  for (const entry of entries) {
    if (entry.key.trim().length === 0) {
      continue;
    }
    lines.push(`${entry.key}\t${entry.text}`);
  }
  if (lines.length === 0) {
    return "";
  }
  return `${lines.join("\n")}\n`;
}

export function buildLocalizationExportFileName(adventurePrefix: string, now = new Date()): string {
  const stamp = formatTimestamp(now);
  return `${adventurePrefix}_localization_${stamp}.txt`;
}

export function formatLocalizationStatus(result: LocalizationGenerationResult): string {
  return `Done. Updated fields: ${result.updatedFieldsCount}. Generated keys: ${result.generatedKeysCount}. Reused keys: ${result.reusedKeysCount}. Export lines: ${result.exportEntries.length}.`;
}

function processScene(sceneData: SceneData, adventurePrefix: string, context: GenerationContext): void {
  const sceneToken = buildToken(sceneData.Id, "SCENE");
  for (let contentIndex = 0; contentIndex < sceneData.Content.length; contentIndex += 1) {
    const content = sceneData.Content[contentIndex];
    if (content == null || content.Type !== "Text") {
      continue;
    }
    const captured = content;
    const index = contentIndex;
    processField(
      captured.Value,
      `${adventurePrefix}_${sceneToken}_CNT_${index + 1}_TEXT`,
      `Scene '${sceneData.Id}' content #${index} Value`,
      context,
      (value) => {
        captured.Value = value;
      },
    );
  }

  for (let choiceIndex = 0; choiceIndex < sceneData.Choices.length; choiceIndex += 1) {
    const choice = sceneData.Choices[choiceIndex];
    if (choice == null) {
      continue;
    }
    processChoice(choice, sceneData.Id, adventurePrefix, sceneToken, choiceIndex, context);
  }
}

function processChoice(
  choice: ChoiceData,
  sceneId: string,
  adventurePrefix: string,
  sceneToken: string,
  choiceIndex: number,
  context: GenerationContext,
): void {
  const choiceBase = `${adventurePrefix}_${sceneToken}_CH_${choiceIndex + 1}`;
  const choiceLabel = choice.Id.trim().length === 0 ? `choice_${choiceIndex}` : choice.Id;
  processField(choice.Text, `${choiceBase}_TEXT`, `Choice '${choiceLabel}' in scene '${sceneId}' Text`, context, (value) => {
    choice.Text = value;
  });
  processField(
    choice.Description,
    `${choiceBase}_DESCR`,
    `Choice '${choiceLabel}' in scene '${sceneId}' Description`,
    context,
    (value) => {
      choice.Description = value;
    },
  );
  if (choice.VisualOptions != null) {
    const visual = choice.VisualOptions;
    processField(
      visual.ParameterOverrideDescription,
      `${choiceBase}_PARAM_OVERRIDE_DESCR`,
      `Choice '${choiceLabel}' in scene '${sceneId}' ParameterOverrideDescription`,
      context,
      (value) => {
        visual.ParameterOverrideDescription = value;
      },
    );
  }
}

function processField(
  fieldValue: string | null,
  baseKey: string,
  path: string,
  context: GenerationContext,
  setValue: (value: string) => void,
): void {
  if (fieldValue == null || fieldValue.trim().length === 0) {
    return;
  }

  const rawValue = fieldValue.trim();
  const existingKey = tryExtractKey(rawValue);
  if (existingKey != null) {
    context.usedKeys.add(existingKey);
    return;
  }

  const reusedKey = context.textToGeneratedKey.get(rawValue);
  if (reusedKey != null) {
    setValue(reusedKey);
    context.reusedKeysCount += 1;
    context.updatedFieldsCount += 1;
    context.fieldChanges.push({
      path,
      originalText: rawValue,
      key: reusedKey,
      kind: "reused",
    });
    return;
  }

  const argumentCount = countUniqueArguments(rawValue);
  const keyedBase = argumentCount > 0 ? `${baseKey}_ARG_${argumentCount}` : baseKey;
  const generatedKey = buildUniqueKey(keyedBase, context.usedKeys);

  setValue(generatedKey);
  context.textToGeneratedKey.set(rawValue, generatedKey);
  context.usedKeys.add(generatedKey);
  context.generatedKeysCount += 1;
  context.updatedFieldsCount += 1;
  context.fieldChanges.push({
    path,
    originalText: rawValue,
    key: generatedKey,
    kind: "generated",
  });
  context.exportEntries.push({
    key: generatedKey,
    text: normalizeTextForTsv(rawValue),
  });
}

function collectAlreadyUsedKeys(adventureData: AdventureData): Set<string> {
  const result = new Set<string>();
  collectKey(adventureData.Title, result);
  collectKey(adventureData.Description, result);

  for (const sceneData of Object.values(adventureData.Scenes)) {
    if (sceneData == null) {
      continue;
    }
    for (const choice of sceneData.Choices) {
      if (choice == null) {
        continue;
      }
      collectKey(choice.Text, result);
      collectKey(choice.Description, result);
      collectKey(choice.VisualOptions?.ParameterOverrideDescription, result);
    }
  }

  return result;
}

function collectKey(value: string | null | undefined, result: Set<string>): void {
  const key = tryExtractKey(value);
  if (key != null) {
    result.add(key);
  }
}

function tryExtractKey(value: string | null | undefined): string | null {
  if (value == null || value.trim().length === 0) {
    return null;
  }
  const trimmed = value.trim();
  const candidate = trimmed.toLowerCase().startsWith(LOC_PREFIX)
    ? trimmed.slice(LOC_PREFIX.length).trim()
    : trimmed;
  if (!KEY_REGEX.test(candidate)) {
    return null;
  }
  return candidate;
}

function countUniqueArguments(value: string): number {
  const indices = new Set<number>();
  ARG_PLACEHOLDER_REGEX.lastIndex = 0;
  let match = ARG_PLACEHOLDER_REGEX.exec(value);
  while (match != null) {
    const rawIndex = match[1];
    if (rawIndex != null) {
      const index = Number.parseInt(rawIndex, 10);
      if (!Number.isNaN(index)) {
        indices.add(index);
      }
    }
    match = ARG_PLACEHOLDER_REGEX.exec(value);
  }
  return indices.size;
}

function buildUniqueKey(baseKey: string, usedKeys: Set<string>): string {
  const sanitizedBase = buildKeyCandidate(baseKey);
  if (!usedKeys.has(sanitizedBase)) {
    return sanitizedBase;
  }
  let suffix = 2;
  while (true) {
    const candidate = `${sanitizedBase}_${suffix}`;
    if (!usedKeys.has(candidate)) {
      return candidate;
    }
    suffix += 1;
  }
}

function buildKeyCandidate(source: string): string {
  let prepared = source.replace(CAMEL_CASE_SPLIT_REGEX, "$1_$2");
  prepared = prepared.replace(NON_ALNUM_REGEX, "_");
  prepared = prepared.replace(MULTI_UNDERSCORE_REGEX, "_").replace(/^_+|_+$/g, "");
  if (prepared.trim().length === 0) {
    prepared = "LOC_KEY";
  }
  return prepared.toUpperCase();
}

function buildAdventurePrefix(selectedFileName: string): string {
  return buildToken(fileNameWithoutExtension(selectedFileName), "ADV");
}

function buildToken(source: string | null | undefined, fallback: string): string {
  if (source == null || source.trim().length === 0) {
    return fallback;
  }
  let prepared = source.replace(CAMEL_CASE_SPLIT_REGEX, "$1_$2");
  prepared = prepared.replace(NON_ALNUM_REGEX, "_");
  prepared = prepared.replace(MULTI_UNDERSCORE_REGEX, "_").replace(/^_+|_+$/g, "");
  if (prepared.trim().length === 0) {
    return fallback;
  }
  prepared = prepared.toUpperCase();
  if (prepared.length > MAX_TOKEN_LENGTH) {
    prepared = prepared.slice(0, MAX_TOKEN_LENGTH);
  }
  return prepared;
}

function fileNameWithoutExtension(fileName: string): string {
  const normalized = fileName.replace(/\\/g, "/");
  const base = normalized.slice(normalized.lastIndexOf("/") + 1);
  const lastDot = base.lastIndexOf(".");
  if (lastDot <= 0) {
    return base;
  }
  return base.slice(0, lastDot);
}

function normalizeTextForTsv(value: string): string {
  if (value.trim().length === 0) {
    return "";
  }
  return value.replace(/\r/g, " ").replace(/\n/g, " ").trim();
}

function compareOrdinal(a: string, b: string): number {
  if (a === b) {
    return 0;
  }
  return a < b ? -1 : 1;
}

function formatTimestamp(now: Date): string {
  const pad = (value: number): string => String(value).padStart(2, "0");
  return `${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}_${pad(now.getHours())}${pad(now.getMinutes())}${pad(now.getSeconds())}`;
}
