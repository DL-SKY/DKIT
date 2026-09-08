import { useMemo, useState } from "react";
import {
  canFixIssue,
  canNavigateToIssue,
  filterValidationIssues,
  type ValidationIssue,
  type ValidationIssueFixFilter,
} from "../domain";

type Props = {
  issues: ValidationIssue[];
  onFix: (issue: ValidationIssue) => void;
  onFixAll: () => void;
  onNavigate: (issue: ValidationIssue) => void;
};

const FIX_FILTERS: { id: ValidationIssueFixFilter; label: string }[] = [
  { id: "all", label: "All" },
  { id: "fixable", label: "Fixable" },
  { id: "unfixable", label: "Unfixable" },
];

export function ValidationPanel({ issues, onFix, onFixAll, onNavigate }: Props) {
  const [query, setQuery] = useState("");
  const [fixFilter, setFixFilter] = useState<ValidationIssueFixFilter>("all");
  const [activeIssueKey, setActiveIssueKey] = useState<string | null>(null);
  const visibleIssues = useMemo(
    () => filterValidationIssues(issues, query, fixFilter),
    [issues, query, fixFilter],
  );
  const fixableCount = issues.filter(canFixIssue).length;
  const filtered = query.trim().length > 0 || fixFilter !== "all";
  const titleCount = filtered ? `${visibleIssues.length} of ${issues.length}` : String(issues.length);

  return (
    <section className="panel validation-panel">
      <div className="validation-header">
        <h2>Validation ({titleCount})</h2>
        <button type="button" data-testid="fix-all" onClick={onFixAll} disabled={fixableCount === 0}>
          Fix All ({fixableCount})
        </button>
      </div>
      <div className="validation-toolbar">
        <input
          className="validation-search"
          data-testid="validation-search"
          type="search"
          value={query}
          placeholder="Search issues"
          onChange={(event) => setQuery(event.target.value)}
        />
        <div className="validation-filters">
          {FIX_FILTERS.map((item) => (
            <button
              key={item.id}
              type="button"
              className={fixFilter === item.id ? "active" : undefined}
              onClick={() => setFixFilter(item.id)}
            >
              {item.label}
            </button>
          ))}
        </div>
      </div>
      {issues.length === 0 ? (
        <p className="hint">No issues.</p>
      ) : visibleIssues.length === 0 ? (
        <p className="hint">No matching issues.</p>
      ) : (
        <ul className="issue-list">
          {visibleIssues.map((issue) => {
            const issueKey = `${issue.message}::${JSON.stringify(issue.location ?? null)}`;
            const navigable = canNavigateToIssue(issue);
            return (
              <li
                key={issueKey}
                className={[
                  "issue-row",
                  navigable ? "navigable" : "",
                  activeIssueKey === issueKey ? "selected" : "",
                ]
                  .filter((item) => item.length > 0)
                  .join(" ")}
              >
                {navigable ? (
                  <button
                    type="button"
                    className="issue-message"
                    onClick={() => {
                      setActiveIssueKey(issueKey);
                      onNavigate(issue);
                    }}
                  >
                    {issue.message}
                  </button>
                ) : (
                  <span className="issue-message">{issue.message}</span>
                )}
                {canFixIssue(issue) ? (
                  <button type="button" onClick={() => onFix(issue)}>
                    Fix
                  </button>
                ) : null}
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
