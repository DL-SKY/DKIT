export function joinLines(items: string[]): string {
  return items.join("\n");
}

export function parseLines(text: string): string[] {
  return text
    .split("\n")
    .map((item) => item.trim())
    .filter((item) => item.length > 0);
}

export function joinNumbers(items: number[] | null): string {
  if (items == null || items.length === 0) {
    return "";
  }
  return items.join("\n");
}

export function parseIntLines(text: string): number[] {
  const result: number[] = [];
  for (const line of text.split("\n")) {
    const token = line.trim();
    if (token.length === 0) {
      continue;
    }
    const value = Number.parseInt(token, 10);
    if (!Number.isNaN(value)) {
      result.push(value);
    }
  }
  return result;
}

export function parseBoolLines(text: string): boolean[] {
  const result: boolean[] = [];
  for (const line of text.split("\n")) {
    const token = line.trim();
    if (token.length === 0) {
      continue;
    }
    if (/^(true|1)$/i.test(token)) {
      result.push(true);
    } else if (/^(false|0)$/i.test(token)) {
      result.push(false);
    }
  }
  return result;
}

export function joinBools(items: boolean[] | null): string {
  if (items == null || items.length === 0) {
    return "";
  }
  return items.map((item) => (item ? "true" : "false")).join("\n");
}

export function uniqueDictionaryKey(
  keys: Iterable<string>,
  requestedKey: string,
  ignoredExistingKey?: string,
): string {
  const existing = new Set(keys);
  const keyBase = requestedKey.trim().length === 0 ? "key" : requestedKey.trim();
  if (!existing.has(keyBase) || keyBase === ignoredExistingKey) {
    return keyBase;
  }
  let suffix = 1;
  while (true) {
    const candidate = `${keyBase}_${suffix}`;
    if (!existing.has(candidate) || candidate === ignoredExistingKey) {
      return candidate;
    }
    suffix += 1;
  }
}
