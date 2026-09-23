import { useState } from "react";
const industries = ["Agriculture", "Education", "Food & Beverage", "Healthcare", "Logistics", "Retail", "Technology", "Other"];
export function DraftStep({ busy, initialDraft = "", initialIndustry = "", onAnalyze }: { busy: boolean; initialDraft?: string; initialIndustry?: string; onAnalyze: (draft: string, industry: string) => void }) {
  const [draft, setDraft] = useState(initialDraft); const [industry, setIndustry] = useState(initialIndustry);
  const valid = draft.trim().length >= 20 && draft.trim().length <= 4000 && Boolean(industry);
  return <section className="wizard-panel draft-panel"><div className="panel-kicker">STEP 1 · START WITH WHAT YOU KNOW</div><h2>Describe the business challenge</h2><p className="panel-copy">A rough description is fine. The coach will help identify details that student teams need.</p>
    <label className="form-label" htmlFor="draft-industry">Industry</label><select id="draft-industry" value={industry} onChange={(e) => setIndustry(e.target.value)}><option value="">Choose an industry…</option>{industries.map((item) => <option key={item}>{item}</option>)}</select>
    <label className="form-label" htmlFor="draft-text">Your task description</label><textarea id="draft-text" className="draft-textarea" minLength={20} maxLength={4000} value={draft} onChange={(e) => setDraft(e.target.value)} placeholder="What is happening today, and what would you like to improve?" />
    <div className="form-hint"><span>Include any details you already know. You can fill gaps later.</span><span>{draft.length} / 4000</span></div>
    {draft.length > 0 && draft.trim().length < 20 && <p className="inline-error">Add at least 20 characters to continue.</p>}
    <button type="button" className="primary-button" disabled={!valid || busy} onClick={() => onAnalyze(draft.trim(), industry)}>{busy ? "Analyzing your description…" : "Analyze with AI →"}</button>
  </section>;
}
