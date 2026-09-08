import { useEffect, useRef, useState } from "react";
import {
  buildLocalizationExportFileName,
  formatLocalizationStatus,
  formatLocalizationTsv,
  generateLocalizationKeys,
  type AdventureData,
  type LocalizationGenerationResult,
} from "../domain";

type Props = {
  adventure: AdventureData;
  fileName: string;
  onApply: (next: AdventureData) => void;
  onClose: () => void;
};

function downloadTextFile(fileName: string, contents: string): void {
  const blob = new Blob([contents], { type: "text/plain;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}

export function LocalizationPanel({ adventure, fileName, onApply, onClose }: Props) {
  const [result, setResult] = useState<LocalizationGenerationResult | null>(null);
  const [applied, setApplied] = useState(false);
  const skipResetRef = useRef(false);

  useEffect(() => {
    if (skipResetRef.current) {
      skipResetRef.current = false;
      return;
    }
    setResult(null);
    setApplied(false);
  }, [adventure, fileName]);

  const generate = () => {
    const next = generateLocalizationKeys(adventure, fileName);
    setResult(next);
    setApplied(false);
  };

  const applyKeys = () => {
    if (result == null || result.updatedFieldsCount === 0) {
      return;
    }
    skipResetRef.current = true;
    onApply(result.adventure);
    setApplied(true);
  };

  const downloadTsv = () => {
    if (result == null) {
      return;
    }
    const exportName = buildLocalizationExportFileName(result.adventurePrefix);
    downloadTextFile(exportName, formatLocalizationTsv(result.exportEntries));
  };

  return (
    <div className="graph-overlay loc-overlay" role="dialog" aria-label="TEA Localization">
      <div className="graph-topbar">
        <span className="graph-title">Localization Export</span>
        <span className="toolbar-spacer" />
        <button type="button" onClick={generate}>
          Generate Localization Keys
        </button>
        <button
          type="button"
          onClick={applyKeys}
          disabled={result == null || result.updatedFieldsCount === 0 || applied}
        >
          Apply Keys
        </button>
        <button type="button" onClick={downloadTsv} disabled={result == null}>
          Download TSV
        </button>
        <button type="button" onClick={onClose}>
          Close
        </button>
      </div>
      <div className="loc-body">
        <p className="hint">
          The window generates localization keys in selected adventure fields and exports a
          tab-separated .txt file for quick paste into Google Sheets. Generate shows a preview.
          Apply writes keys into the current adventure. Download saves the .txt locally.
        </p>
        {result == null ? (
          <p className="hint">Choose Generate Localization Keys to preview changes.</p>
        ) : (
          <>
            <p className={applied ? "status saved" : "status modified"}>{formatLocalizationStatus(result)}</p>
            {applied ? <p className="hint">Keys applied. Save / Save As to keep them in the JSON file.</p> : null}
            {result.fieldChanges.length === 0 ? (
              <p className="hint">No fields to update. Existing keys were left as-is.</p>
            ) : (
              <table className="loc-table">
                <thead>
                  <tr>
                    <th>Field</th>
                    <th>Original</th>
                    <th>Key</th>
                    <th>Kind</th>
                  </tr>
                </thead>
                <tbody>
                  {result.fieldChanges.map((change) => (
                    <tr key={`${change.path}:${change.key}:${change.kind}`}>
                      <td>{change.path}</td>
                      <td title={change.originalText}>{change.originalText}</td>
                      <td>
                        <code>{change.key}</code>
                      </td>
                      <td>{change.kind}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </>
        )}
      </div>
    </div>
  );
}
