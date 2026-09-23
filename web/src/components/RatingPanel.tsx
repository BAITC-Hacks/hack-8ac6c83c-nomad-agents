import type { CSSProperties } from "react";
import { LevelBadge, type ReadinessLevel } from "./LevelBadge";

export type RatingBreakdownItem = {
  criterion: string;
  weight: number;
  score: number;
  reason: string;
  matchedSignals?: string[];
};

export type RatingQuest = {
  criterion: string;
  fieldKey: string;
  action: string;
  potentialPoints: number;
};

export type RatingDto = {
  total: number;
  level: ReadinessLevel;
  breakdown: RatingBreakdownItem[];
  missingDetails: { criterion: string; detail: string }[];
  quests: RatingQuest[];
  source: "rules" | "seed" | "cache";
  ratingRulesVersion?: string;
  scoredAt?: string;
  nextLevel?: { level: string; pointsNeeded: number };
};

export type RatingPanelProps = {
  rating: RatingDto;
  variant?: "full" | "compact";
  /** Difference from the preceding confirmed score; omit for previews and first confirmations. */
  delta?: number | null;
  /** Level from the preceding confirmed score. Used only to show a genuine threshold crossing. */
  previousLevel?: ReadinessLevel | null;
  position?: { rank: number; total: number } | null;
  onAddDetails?: (fieldKey: string) => void;
};

const LEVEL_ORDER: ReadinessLevel[] = ["draft", "workable", "ready", "priority"];
const LEVEL_THRESHOLDS: Record<ReadinessLevel, number> = {
  draft: 0,
  workable: 40,
  ready: 70,
  priority: 90,
};
const LEVEL_NAMES: Record<ReadinessLevel, string> = {
  draft: "Needs clarification",
  workable: "Workable",
  ready: "Ready",
  priority: "Priority",
};

const sourceLabel = { rules: "Deterministic rules", seed: "Seeded rating", cache: "Cached rating" } as const;

function getNextLevel(rating: RatingDto) {
  const next = LEVEL_ORDER.find((level) => LEVEL_THRESHOLDS[level] > rating.total);
  return next ? { level: next, pointsNeeded: LEVEL_THRESHOLDS[next] - rating.total } : null;
}

function MiniBar({ score, weight }: { score: number; weight: number }) {
  const percent = weight > 0 ? Math.max(0, Math.min(100, (score / weight) * 100)) : 0;
  return (
    <span aria-label={`${score} of ${weight} points`} style={{ display: "inline-block", width: 76, height: 7, background: "#e2e8f0", borderRadius: 8, verticalAlign: "middle", overflow: "hidden" }}>
      <span style={{ display: "block", height: "100%", width: `${percent}%`, background: "#2563eb" }} />
    </span>
  );
}

export function RatingPanel({ rating, variant = "full", delta, previousLevel, position, onAddDetails }: RatingPanelProps) {
  const nextLevel = getNextLevel(rating);
  const crossedLevel = previousLevel && LEVEL_ORDER.indexOf(rating.level) > LEVEL_ORDER.indexOf(previousLevel);
  const quests = [...(rating.quests ?? [])].sort((a, b) => b.potentialPoints - a.potentialPoints);
  const muted: CSSProperties = { color: "#64748b" };

  return (
    <section aria-label="Challenge rating" style={{ border: "1px solid #e2e8f0", borderRadius: 12, padding: "1rem", background: "white" }}>
      <header style={{ display: "flex", flexWrap: "wrap", alignItems: "center", gap: 10 }}>
        <strong style={{ fontSize: "2rem", lineHeight: 1 }}>{rating.total}<span style={{ fontSize: "1rem", color: "#64748b" }}>/100</span></strong>
        <LevelBadge level={rating.level} />
        {delta !== undefined && delta !== null && (
          <span aria-label={`Confirmed score change ${delta > 0 ? "plus " : "minus "}${Math.abs(delta)} points`} style={{ color: delta >= 0 ? "#15803d" : "#b91c1c", fontWeight: 700 }}>
            {delta > 0 ? `+${delta} ▲` : delta < 0 ? `−${Math.abs(delta)} ▼` : "No change"}
          </span>
        )}
        {position && <span style={{ marginLeft: "auto", fontWeight: 700 }}>Catalog position #{position.rank} of {position.total}</span>}
      </header>

      <div style={{ position: "relative", margin: "1.2rem 4px 1.6rem" }}>
        <div role="progressbar" aria-label="Readiness score" aria-valuemin={0} aria-valuemax={100} aria-valuenow={rating.total} style={{ height: 10, borderRadius: 10, background: "#e2e8f0", overflow: "hidden" }}>
          <div style={{ height: "100%", width: `${Math.max(0, Math.min(100, rating.total))}%`, background: rating.level === "priority" ? "#d4a017" : "#2563eb", transition: "width 180ms ease" }} />
        </div>
        {[40, 70, 90].map((threshold) => (
          <span key={threshold} aria-label={`${threshold} point level threshold`} title={`${threshold} points`} style={{ position: "absolute", left: `${threshold}%`, top: -3, width: 2, height: 16, background: "#334155" }} />
        ))}
        <div style={{ display: "flex", justifyContent: "space-between", marginTop: 5, ...muted, fontSize: "0.72rem" }}><span>0</span><span>40</span><span>70</span><span>90</span><span>100</span></div>
      </div>

      {crossedLevel && (
        <div role="status" style={{ margin: "0.8rem 0", padding: "0.7rem", borderRadius: 8, background: "#f0fdf4", color: "#166534", fontWeight: 700 }}>
          Level up: {LEVEL_NAMES[previousLevel]} → {LEVEL_NAMES[rating.level]}!
        </div>
      )}
      {delta !== undefined && delta !== null && (
        <p style={{ margin: "0.5rem 0", fontSize: "0.9rem" }}>
          Confirmed score: {delta >= 0 ? `+${delta}` : `−${Math.abs(delta)}`} points{previousLevel ? ` · ${LEVEL_NAMES[previousLevel]} → ${LEVEL_NAMES[rating.level]}` : ""}
        </p>
      )}
      {nextLevel ? <p style={{ margin: "0.5rem 0", fontWeight: 600 }}>Next level: {LEVEL_NAMES[nextLevel.level]} — {nextLevel.pointsNeeded} points needed</p> : <p style={{ margin: "0.5rem 0", fontWeight: 600 }}>Top readiness level reached</p>}

      {variant === "full" && (
        <>
          <h3 style={{ margin: "1.2rem 0 0.5rem" }}>Rating breakdown</h3>
          <div style={{ overflowX: "auto" }}>
            <table style={{ borderCollapse: "collapse", width: "100%", fontSize: "0.9rem" }}>
              <thead><tr style={{ textAlign: "left", borderBottom: "1px solid #cbd5e1" }}><th style={{ padding: "0.5rem 0" }}>Criterion</th><th>Score</th><th>Reason</th></tr></thead>
              <tbody>{rating.breakdown.map((item) => (
                <tr key={item.criterion} style={{ borderBottom: "1px solid #f1f5f9" }}>
                  <td style={{ padding: "0.55rem 0" }}>{item.criterion}</td>
                  <td style={{ whiteSpace: "nowrap" }}><MiniBar score={item.score} weight={item.weight} /> <span>{item.score}/{item.weight}</span></td>
                  <td style={{ padding: "0.55rem 0.4rem" }}>{item.reason}</td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          {!!rating.missingDetails?.length && <div style={{ marginTop: "0.8rem" }}><strong>Missing details</strong><ul>{rating.missingDetails.map((item, i) => <li key={`${item.criterion}-${i}`}>{item.detail}</li>)}</ul></div>}

          <h3 style={{ margin: "1.2rem 0 0.3rem" }}>Improve Your Challenge</h3>
          <p style={{ ...muted, marginTop: 0, fontSize: "0.85rem" }}>Full points require the evidence listed for each field.</p>
          {quests.length ? <ul style={{ listStyle: "none", margin: 0, padding: 0, display: "grid", gap: 8 }}>
            {quests.map((quest, index) => <li key={`${quest.fieldKey}-${index}`} style={{ display: "flex", flexWrap: "wrap", alignItems: "center", gap: 8, padding: "0.65rem", background: "#f8fafc", borderRadius: 8 }}>
              <span style={{ flex: "1 1 220px" }}>{quest.action}</span><strong style={{ whiteSpace: "nowrap" }}>+{quest.potentialPoints} points</strong>
              {onAddDetails && <button type="button" onClick={() => onAddDetails(quest.fieldKey)} style={{ cursor: "pointer" }}>Add details</button>}
            </li>)}
          </ul> : <p style={muted}>No improvement quests right now.</p>}
        </>
      )}
      <footer style={{ marginTop: "1rem", display: "flex", justifyContent: "space-between", gap: 8, ...muted, fontSize: "0.8rem" }}>
        <span>{sourceLabel[rating.source] ?? "Rating"}</span>
        {rating.ratingRulesVersion && <span>Rules {rating.ratingRulesVersion}</span>}
      </footer>
    </section>
  );
}
