import type { RestrictionType } from "./enums";

export const RESTRICTION_EDITOR_FIELDS = {
  CompareOptions: "CompareOptions",
  StringValues: "StringValues",
  IntValues: "IntValues",
  LongValues: "LongValues",
  BoolValues: "BoolValues",
} as const;

export type RestrictionEditorField = (typeof RESTRICTION_EDITOR_FIELDS)[keyof typeof RESTRICTION_EDITOR_FIELDS];

const DEFAULT_FIELDS: RestrictionEditorField[] = [
  RESTRICTION_EDITOR_FIELDS.CompareOptions,
  RESTRICTION_EDITOR_FIELDS.StringValues,
  RESTRICTION_EDITOR_FIELDS.IntValues,
  RESTRICTION_EDITOR_FIELDS.LongValues,
  RESTRICTION_EDITOR_FIELDS.BoolValues,
];

const PROFILES_BY_TYPE: Partial<Record<RestrictionType, RestrictionEditorField[]>> = {
  TimeNow: [RESTRICTION_EDITOR_FIELDS.CompareOptions, RESTRICTION_EDITOR_FIELDS.LongValues],
  ActivePartyCount: [RESTRICTION_EDITOR_FIELDS.CompareOptions, RESTRICTION_EDITOR_FIELDS.IntValues],
  CharacterParams: [
    RESTRICTION_EDITOR_FIELDS.CompareOptions,
    RESTRICTION_EDITOR_FIELDS.StringValues,
    RESTRICTION_EDITOR_FIELDS.IntValues,
  ],
};

export function getVisibleRestrictionFields(type: RestrictionType): RestrictionEditorField[] {
  return PROFILES_BY_TYPE[type] ?? DEFAULT_FIELDS;
}

export function hasRestrictionEditorField(
  type: RestrictionType,
  field: RestrictionEditorField,
): boolean {
  return getVisibleRestrictionFields(type).includes(field);
}
