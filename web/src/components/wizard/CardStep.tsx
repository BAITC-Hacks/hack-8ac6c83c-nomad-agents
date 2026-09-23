import { useState } from "react";
import { RatingPanel, type RatingDto } from "../RatingPanel";
import type { Analysis, CardFields, Rating } from "../../pages/business/NewTaskWizard";
import { labelForField } from "../../pages/business/NewTaskWizard";
const fields: { key: keyof CardFields; label: string; hint: string; multiline?: boolean; maxLength: number; tags?: boolean }[] = [
  { key: "title", label: "Title", hint: "A short, neutral name for this challenge.", maxLength: 120 },
  { key: "context", label: "Context", hint: "What is happening now?", multiline: true, maxLength: 2000 },
  { key: "need", label: "Need", hint: "What needs to change?", multiline: true, maxLength: 2000 },
  { key: "users", label: "Users", hint: "Who will use or benefit from the solution?", multiline: true, maxLength: 1000 },
  { key: "data", label: "Data & materials", hint: "What data, examples, or materials are available?", multiline: true, maxLength: 2000 },
  { key: "constraints", label: "Constraints", hint: "Deadlines, technology, access, or other limits.", multiline: true, maxLength: 1500 },
  { key: "expectedResult", label: "Expected result", hint: "What should the team deliver?", multiline: true, maxLength: 1500 },
  { key: "successCriteria", label: "Success criteria", hint: "How will you know the result works?", multiline: true, maxLength: 1500 },
  { key: "contact", label: "Contact", hint: "Who can the team contact?", maxLength: 200 },
  { key: "interactionFormat", label: "Interaction format", hint: "How will consultation and feedback work?", multiline: true, maxLength: 1000 },
  { key: "topics", label: "Topics / industry tags", hint: "Separate tags with commas.", maxLength: 300, tags: true },
  { key: "techTags", label: "Technology tags", hint: "Separate tags with commas.", maxLength: 300, tags: true },
];
export function CardStep({ fields: values, analysis, rating, busy, onChange, onSave, onConfirm, onFocusField }: { fields: CardFields; analysis: Analysis | null; rating: Rating | null; busy: boolean; onChange: (key: keyof CardFields, value: string | string[]) => void; onSave: (fields: CardFields) => Promise<void>; onConfirm: (fields: CardFields) => void; onFocusField: (key: string) => void }) {
  const [saved, setSaved] = useState(false);
  const [saveError, setSaveError] = useState(false);
  const useValue = (key: keyof CardFields, value: string) => {
    const current = values[key];
    onChange(key, Array.isArray(current) ? [...current, value] : current ? `${current}\n${value}` : value);
  };
  const save = async () => { setSaveError(false); try { await onSave(values); setSaved(true); } catch { setSaveError(true); } };
  return <div className="card-editor-layout"><section className="wizard-panel card-form-panel"><div className="panel-kicker">STEP 3 · TASK CARD</div><h2>Review and complete the card</h2><p className="panel-copy">The business owns this content. Check every suggestion and edit details before confirming.</p>
    {analysis?.title && !values.title && <div className="suggested-title"><div><span className="suggestion-tag">TITLE SUGGESTION</span><p>{analysis.title}</p><small>Suggested from your description. Review before using.</small></div><button type="button" className="small-use" onClick={() => onChange("title", analysis.title)}>Use</button></div>}
    {analysis?.extracted.length ? <div className="extract-list"><h3>Details found in your description</h3>{analysis.extracted.map((item, i) => <div className="extract-item" key={`${item.fieldKey}-${i}`}><div><b>{labelForField(item.fieldKey)}</b><p>{item.value}</p><small>Evidence: “{item.evidence}”</small></div><button type="button" className="small-use" onClick={() => useValue(item.fieldKey as keyof CardFields, item.value)}>Use</button></div>)}</div> : null}
    <div className="field-grid">{fields.map((field) => <label className={`card-field ${field.key === "title" ? "wide" : ""}`} htmlFor={`field-${field.key}`} key={field.key}><span>{field.label}{field.key === "title" && <i>Required to publish</i>}{field.tags && <i>{values[field.key].length} / {field.key === "topics" ? 5 : 8} tags</i>}</span>{field.tags ? <input id={`field-${field.key}`} maxLength={field.maxLength} value={(values[field.key] as string[]).join(", ")} onChange={(e) => { const tags = e.target.value.split(",").map((tag) => tag.trim()).filter(Boolean).slice(0, field.key === "topics" ? 5 : 8); onChange(field.key, tags); setSaved(false); }} placeholder={field.hint} /> : field.multiline ? <textarea id={`field-${field.key}`} rows={3} maxLength={field.maxLength} value={values[field.key] as string} onChange={(e) => { onChange(field.key, e.target.value); setSaved(false); }} placeholder={field.hint} /> : <input id={`field-${field.key}`} maxLength={field.maxLength} value={values[field.key] as string} onChange={(e) => { onChange(field.key, e.target.value); setSaved(false); }} placeholder={field.hint} />}</label>)}</div>
    <div className="card-actions"><button type="button" className="secondary-button" disabled={busy} onClick={save}>Save card edits</button><button type="button" className="primary-button" disabled={busy || !values.title.trim()} onClick={() => onConfirm(values)}>{busy ? "Confirming…" : "Confirm & score →"}</button></div>{saved && <p className="saved-note">Card edits saved.</p>}{saveError && <p className="inline-error">Could not save the card. Your edits are still here.</p>}
  </section><aside className="coach-sidebar"><div className="sidebar-label">YOUR READINESS</div>{rating ? <RatingPanel rating={rating as RatingDto} variant="full" onAddDetails={onFocusField} /> : <div className="rating-empty"><span>◉</span><h3>Score your task card</h3><p>Confirm your card to see its readiness score, field breakdown, and suggested improvements.</p></div>}
    {rating?.quests.length ? <div className="quest-shortlist"><h3>Next improvements</h3>{rating.quests.slice(0, 3).map((quest, i) => <button key={`${quest.fieldKey}-${i}`} type="button" onClick={() => onFocusField(quest.fieldKey)}><span>+{quest.potentialPoints}</span><span>{quest.action}<small>Add details → {labelForField(quest.fieldKey)}</small></span></button>)}</div> : null}
  </aside></div>;
}
