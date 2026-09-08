import {
  COMPARE_TYPES,
  RESTRICTION_TYPES,
  createDefaultRestriction,
  hasRestrictionEditorField,
  type CompareType,
  type Restriction,
  type RestrictionType,
} from "../domain";
import { joinBools, joinLines, joinNumbers, parseBoolLines, parseIntLines, parseLines } from "./lists";

type Props = {
  restrictions: Restriction[];
  onChange: (next: Restriction[]) => void;
};

function patchRestriction(restriction: Restriction, patch: Partial<Restriction>): Restriction {
  return { ...restriction, ...patch };
}

export function RestrictionsEditor({ restrictions, onChange }: Props) {
  return (
    <div className="restrictions-editor">
      <h3>Restrictions</h3>
      <div className="button-row">
        <button type="button" onClick={() => onChange([...restrictions, createDefaultRestriction()])}>
          Add Restriction
        </button>
      </div>
      {restrictions.map((restriction, index) => (
        <div key={index} className="restriction-card">
          <div className="restriction-card-header">
            <strong>Restriction {index + 1}</strong>
            <button
              type="button"
              onClick={() => onChange(restrictions.filter((_, itemIndex) => itemIndex !== index))}
            >
              X
            </button>
          </div>
          <label className="field">
            <span>Type</span>
            <select
              value={restriction.Type}
              onChange={(event) =>
                onChange(
                  restrictions.map((item, itemIndex) =>
                    itemIndex === index
                      ? patchRestriction(item, { Type: event.target.value as RestrictionType })
                      : item,
                  ),
                )
              }
            >
              {RESTRICTION_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </label>
          {hasRestrictionEditorField(restriction.Type, "CompareOptions") ? (
            <label className="field">
              <span>Compare</span>
              <select
                value={restriction.CompareOptions}
                onChange={(event) =>
                  onChange(
                    restrictions.map((item, itemIndex) =>
                      itemIndex === index
                        ? patchRestriction(item, { CompareOptions: event.target.value as CompareType })
                        : item,
                    ),
                  )
                }
              >
                {COMPARE_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </label>
          ) : null}
          {hasRestrictionEditorField(restriction.Type, "StringValues") ? (
            <label className="field">
              <span>Strings</span>
              <textarea
                rows={2}
                value={joinLines(restriction.StringValues ?? [])}
                onChange={(event) =>
                  onChange(
                    restrictions.map((item, itemIndex) =>
                      itemIndex === index
                        ? patchRestriction(item, { StringValues: parseLines(event.target.value) })
                        : item,
                    ),
                  )
                }
              />
            </label>
          ) : null}
          {hasRestrictionEditorField(restriction.Type, "IntValues") ? (
            <label className="field">
              <span>Ints</span>
              <textarea
                rows={2}
                value={joinNumbers(restriction.IntValues)}
                onChange={(event) =>
                  onChange(
                    restrictions.map((item, itemIndex) =>
                      itemIndex === index
                        ? patchRestriction(item, { IntValues: parseIntLines(event.target.value) })
                        : item,
                    ),
                  )
                }
              />
            </label>
          ) : null}
          {hasRestrictionEditorField(restriction.Type, "LongValues") ? (
            <label className="field">
              <span>Longs</span>
              <textarea
                rows={2}
                value={joinNumbers(restriction.LongValues)}
                onChange={(event) =>
                  onChange(
                    restrictions.map((item, itemIndex) =>
                      itemIndex === index
                        ? patchRestriction(item, { LongValues: parseIntLines(event.target.value) })
                        : item,
                    ),
                  )
                }
              />
            </label>
          ) : null}
          {hasRestrictionEditorField(restriction.Type, "BoolValues") ? (
            <label className="field">
              <span>Bools</span>
              <textarea
                rows={2}
                value={joinBools(restriction.BoolValues)}
                onChange={(event) =>
                  onChange(
                    restrictions.map((item, itemIndex) =>
                      itemIndex === index
                        ? patchRestriction(item, { BoolValues: parseBoolLines(event.target.value) })
                        : item,
                    ),
                  )
                }
              />
            </label>
          ) : null}
        </div>
      ))}
    </div>
  );
}
