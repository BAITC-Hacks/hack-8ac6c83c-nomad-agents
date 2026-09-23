import type { Analysis } from "../../pages/business/NewTaskWizard";
import { labelForField } from "../../pages/business/NewTaskWizard";
export function ClarifyStep({ analysis, answers, appliedAnswers, busy, onAnswer, onApply, onEditCard }: { analysis: Analysis; answers: Record<string, string>; appliedAnswers: { questionId: string; fieldKey: string; text: string }[]; busy: boolean; onAnswer: (id: string, value: string) => void; onApply: () => void; onEditCard: () => void }) {
  const applied = appliedAnswers.length > 0;
  return <section className="wizard-coach-layout"><div className="wizard-panel coach-panel"><div className="coach-title"><div><span className="coach-mark">✳</span><div><div className="panel-kicker">AI CHALLENGE COACH</div><h2>Clarify the challenge</h2></div></div><span className={`source-badge ${analysis.source}`}>{analysis.source === "stub" ? "BASIC MODE" : "AI ANALYSIS"}</span></div>
    {analysis.source === "stub" && <div className="stub-banner">AI unavailable — basic questions; you can continue editing.</div>}
    <p className="panel-copy">Your answers are added to the card for you to review and edit.</p>
    {analysis.questions.map((item, index) => <article className="question-card" key={item.id}><div className="question-number">{String(index + 1).padStart(2, "0")}</div><div className="question-content"><div className="field-label">{labelForField(item.fieldKey)}</div><h3>{item.question}</h3>
      {!applied && item.chips.slice(0, 4).length > 0 && <div className="answer-chips" aria-label="Answer suggestions">{item.chips.slice(0, 4).map((chip) => <button type="button" key={chip} disabled={applied} className={`answer-chip${answers[item.id] === chip ? " selected" : ""}`} onClick={() => onAnswer(item.id, answers[item.id] === chip ? "" : chip)}>{chip}</button>)}</div>}
      {applied ? <div className="applied-answer">{appliedAnswers.find((answer) => answer.questionId === item.id)?.text || <em>No answer added</em>}</div> : <textarea aria-label={`Your answer: ${item.question}`} rows={2} value={answers[item.id] ?? ""} onChange={(e) => onAnswer(item.id, e.target.value)} placeholder="Write your answer or choose an option above…" />}
    </div></article>)}
    <div className="suggestions-block"><h3>Coach suggestions</h3><p>Consider adding these details to strengthen the task.</p><ul>{analysis.suggestions.map((suggestion, i) => <li key={`${suggestion.fieldKey}-${i}`}><span>↗</span><div><b>{labelForField(suggestion.fieldKey)}</b><br />{suggestion.action}</div></li>)}</ul></div>
    {applied ? <div className="applied-footer"><span>✓ Answers applied. Changes now belong in the card editor.</span><button type="button" className="secondary-button" onClick={onEditCard}>Continue to card →</button></div> : <button type="button" className="primary-button" disabled={busy || analysis.questions.length < 3} onClick={onApply}>{busy ? "Applying answers…" : "Apply answers →"}</button>}
  </div></section>;
}
