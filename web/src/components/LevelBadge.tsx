import type { CSSProperties } from "react";

export type ReadinessLevel = "draft" | "workable" | "ready" | "priority";

const LEVEL_LABELS: Record<ReadinessLevel, string> = {
  draft: "Needs clarification",
  workable: "Workable",
  ready: "Ready",
  priority: "Priority",
};

const LEVEL_STYLES: Record<ReadinessLevel, CSSProperties> = {
  draft: { color: "#475569", background: "#e2e8f0" },
  workable: { color: "#1d4ed8", background: "#dbeafe" },
  ready: { color: "#15803d", background: "#dcfce7" },
  priority: { color: "#a16207", background: "#fef3c7" },
};

export function LevelBadge({ level }: { level: ReadinessLevel }) {
  return (
    <span
      aria-label={`Readiness: ${LEVEL_LABELS[level]}`}
      style={{
        ...LEVEL_STYLES[level],
        display: "inline-flex",
        alignItems: "center",
        borderRadius: 999,
        padding: "0.2rem 0.65rem",
        fontSize: "0.8rem",
        fontWeight: 700,
        whiteSpace: "nowrap",
      }}
    >
      {LEVEL_LABELS[level]}
    </span>
  );
}
