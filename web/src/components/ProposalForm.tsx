import { useState, type FormEvent } from "react";
import { apiFetch } from "../lib/http";
import type { ProposalSummary } from "../pages/types";

export default function ProposalForm({ taskId, proposal, readOnly }: { taskId: string; proposal: ProposalSummary | null; readOnly: boolean }) {
  const [idea, setIdea] = useState(proposal?.idea ?? "");
  const [plan, setPlan] = useState(proposal?.plan ?? "");
  const [timeline, setTimeline] = useState(proposal?.timeline ?? "");
  const [prototypeUrl, setPrototypeUrl] = useState(proposal?.prototypeUrl ?? "");
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const submit = async (event: FormEvent) => {
    event.preventDefault(); setSaving(true); setMessage("");
    try {
      await apiFetch(`/api/tasks/${encodeURIComponent(taskId)}/proposals/mine`, { method: "PUT", body: JSON.stringify({ idea: idea.trim(), plan: plan.trim(), timeline: timeline.trim(), prototypeUrl: prototypeUrl.trim() }) });
      setMessage("Proposal saved. Open My proposals to track its status.");
    } catch (error) { setMessage(error instanceof Error ? error.message : "Could not save proposal."); }
    finally { setSaving(false); }
  };
  return <form onSubmit={submit} className="proposal-form">
    <label>Solution idea<textarea required minLength={20} maxLength={2000} value={idea} onChange={e => setIdea(e.target.value)} disabled={readOnly} rows={4} /></label>
    <label>Implementation plan<textarea required minLength={20} maxLength={3000} value={plan} onChange={e => setPlan(e.target.value)} disabled={readOnly} rows={5} /></label>
    <label>Estimated duration<input required minLength={3} maxLength={200} value={timeline} onChange={e => setTimeline(e.target.value)} disabled={readOnly} placeholder="e.g. 5 weeks" /></label>
    <label>Prototype URL (optional)<input type="url" pattern="https?://.*" value={prototypeUrl} onChange={e => setPrototypeUrl(e.target.value)} disabled={readOnly} placeholder="https://…" /></label>
    {!readOnly && <button className="primary-button" disabled={saving}>{saving ? "Saving…" : proposal ? "Update proposal" : "Submit proposal"}</button>}
    {message && <p role="status">{message}</p>}
  </form>;
}
