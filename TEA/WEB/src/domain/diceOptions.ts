import { DICE_OPTIONS, type DiceOptionFlag, type DiceOptions } from "./enums";

export const DICE_OPTION_FLAGS: DiceOptionFlag[] = DICE_OPTIONS.filter(
  (option): option is DiceOptionFlag => option !== "None",
);

export function parseDiceOptionFlags(value: DiceOptions | string): DiceOptionFlag[] {
  const tokens = String(value)
    .split(",")
    .map((token) => token.trim())
    .filter((token) => token.length > 0 && token !== "None");
  return DICE_OPTION_FLAGS.filter((flag) => tokens.includes(flag));
}

export function formatDiceOptions(flags: readonly string[]): DiceOptions {
  const selected = DICE_OPTION_FLAGS.filter((flag) => flags.includes(flag));
  if (selected.length === 0) {
    return "None";
  }
  return selected.join(", ") as DiceOptions;
}
