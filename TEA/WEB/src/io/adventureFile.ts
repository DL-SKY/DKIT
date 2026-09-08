import { normalizeAdventure } from "../domain/normalize";
import { serializeAdventure } from "../domain/serialize";
import type { AdventureData } from "../domain/types";

const JSON_PICKER_TYPES: FilePickerAcceptType[] = [
  {
    description: "Adventure JSON",
    accept: {
      "application/json": [".json"],
    },
  },
];

export type OpenedAdventureFile = {
  adventure: AdventureData;
  fileName: string;
  handle: FileSystemFileHandle | null;
};

export function canUseFileSystemAccess(): boolean {
  return (
    typeof window.showOpenFilePicker === "function" &&
    typeof window.showSaveFilePicker === "function"
  );
}

export function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError";
}

function parseAdventureJson(text: string): AdventureData {
  let parsed: unknown;
  try {
    parsed = JSON.parse(text);
  } catch (error) {
    const message = error instanceof Error ? error.message : "Invalid JSON.";
    throw new Error(`Failed to parse JSON: ${message}`);
  }

  if (parsed == null || typeof parsed !== "object" || Array.isArray(parsed)) {
    throw new Error("JSON root must be an adventure object.");
  }

  return normalizeAdventure(parsed);
}

export async function openAdventureFromHandle(
  handle: FileSystemFileHandle,
): Promise<OpenedAdventureFile> {
  const file = await handle.getFile();
  const text = await file.text();
  return {
    adventure: parseAdventureJson(text),
    fileName: file.name,
    handle,
  };
}

export async function openAdventureWithPicker(): Promise<OpenedAdventureFile | null> {
  if (typeof window.showOpenFilePicker !== "function") {
    return null;
  }
  const [handle] = await window.showOpenFilePicker({
    multiple: false,
    types: JSON_PICKER_TYPES,
  });
  if (handle == null) {
    return null;
  }
  return openAdventureFromHandle(handle);
}

export async function openAdventureFromFile(file: File): Promise<OpenedAdventureFile> {
  const text = await file.text();
  return {
    adventure: parseAdventureJson(text),
    fileName: file.name,
    handle: null,
  };
}

function downloadJson(fileName: string, contents: string): void {
  const blob = new Blob([contents], { type: "application/json;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName.endsWith(".json") ? fileName : `${fileName}.json`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}

export async function saveAdventureToHandle(
  handle: FileSystemFileHandle,
  adventure: AdventureData,
): Promise<void> {
  const writable = await handle.createWritable();
  await writable.write(serializeAdventure(adventure));
  await writable.close();
}

export async function saveAdventureAs(
  adventure: AdventureData,
  suggestedName: string,
): Promise<{ fileName: string; handle: FileSystemFileHandle | null }> {
  const contents = serializeAdventure(adventure);
  const fileName = suggestedName.endsWith(".json") ? suggestedName : `${suggestedName}.json`;

  if (typeof window.showSaveFilePicker === "function") {
    const handle = await window.showSaveFilePicker({
      suggestedName: fileName,
      types: JSON_PICKER_TYPES,
    });
    await saveAdventureToHandle(handle, adventure);
    return { fileName: handle.name, handle };
  }

  downloadJson(fileName, contents);
  return { fileName, handle: null };
}
