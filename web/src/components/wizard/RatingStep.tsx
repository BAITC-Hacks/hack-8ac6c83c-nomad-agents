import { RatingPanel, type RatingDto } from "../RatingPanel";
import type { CardFields, Rating } from "../../pages/business/NewTaskWizard";
function levelName(level: string) { return ({ draft: "Needs clarification", workable: "Workable", ready: "Ready", priority: "Priority" } as Record<string, string>)[level] ?? level; }
export function RatingStep({ current, previous, delta, fields, busy, onImprove, onPublish }: { current: Rating; previous: Rating | null; delta: number; fields: CardFields; busy: boolean; onImprove: () => void; onPublish: () => void }) {
  const crossed = Boolean(previous && previous.level !== current.level && current.total > previous.total);
  return <section className="wizard-panel rating-step"><div className="panel-kicker">STEP 4 · CONFIRMED SCORE</div><h2>Your task is more ready for teams</h2><p className="panel-copy">This score reflects the confirmed card. It changes after you edit and confirm again.</p>
    <div className="score-comparison"><div><span>PREVIOUS CONFIRMATION</span><strong>{previous ? `${previous.total}` : "—"}</strong><small>{previous ? levelName(previous.level) : "First confirmation"}</small></div><span className="comparison-arrow">→</span><div className="current-score"><span>CURRENT CONFIRMATION</span><strong>{current.total}<small>/100</small></strong><small>{levelName(current.level)}</small></div><div className={`delta-pill ${delta >= 0 ? "positive" : "negative"}`}>{delta > 0 ? "+" : ""}{delta} points</div></div>
    {crossed && <div className="level-up-notice"><span>↗</span><div><b>Level up: {levelName(previous!.level)} → {levelName(current.level)}</b><small>Your confirmed task crossed a readiness threshold.</small></div></div>}
    <div className="rating-panel-wrap"><RatingPanel rating={current as RatingDto} variant="full" delta={delta} previousLevel={previous?.level ?? null} /></div>
    <div className="rating-step-footer"><div><b>{fields.title}</b><span>{fields.topics.join(" · ")}</span></div><div className="rating-buttons"><button type="button" className="secondary-button" onClick={onImprove}>← Improve card</button><button type="button" className="primary-button" disabled={busy || !fields.title.trim()} onClick={onPublish}>{busy ? "Publishing…" : "Publish task →"}</button></div></div>
  </section>;
}
