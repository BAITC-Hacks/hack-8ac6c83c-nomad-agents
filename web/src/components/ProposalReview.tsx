import { useCallback, useEffect, useRef, useState } from "react";
import { apiFetch } from "../lib/http";

type Proposal = { id: string; teamName: string; tags?: string[]; idea: string; plan: string; timeline: string; prototypeUrl?: string; status: "pending" | "selected" | "rejected"; reason?: string; milestones?: { id: string; title: string }[] };
export default function ProposalReview({ taskId }: { taskId: string }) {
  const [proposals, setProposals] = useState<Proposal[]>([]);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState("");
  const [reason, setReason] = useState<Record<string, string>>({});
  const [milestone, setMilestone] = useState<Record<string, string>>({});
  const milestoneKeys = useRef<Record<string, string>>({});
  const load = useCallback(() => apiFetch<Proposal[]>(`/api/tasks/${encodeURIComponent(taskId)}/proposals`).then(setProposals).catch(e => setError(e instanceof Error ? e.message : "Could not load proposals.")), [taskId]);
  useEffect(() => { void load(); }, [load]);
  const decide = async (id: string, decision: Proposal["status"]) => {
    setBusy(id); setError("");
    try { await apiFetch(`/api/proposals/${id}/decision`, { method: "POST", body: JSON.stringify({ decision, reason: reason[id] ?? "" }) }); await load(); }
    catch (e) { setError(e instanceof Error ? e.message : "Could not save decision."); }
    finally { setBusy(""); }
  };
  const confirmMilestone = async (id: string) => {
    const title = milestone[id]?.trim(); if (!title) { setError("Enter a milestone title."); return; }
    if (busy === id) return;
    const key = milestoneKeys.current[id] ?? crypto.randomUUID(); milestoneKeys.current[id] = key; setBusy(id); setError("");
    try { await apiFetch(`/api/proposals/${id}/milestones`, { method: "POST", body: JSON.stringify({ id: key, title }) }); delete milestoneKeys.current[id]; setMilestone(previous => ({ ...previous, [id]: "" })); await load(); }
    catch (e) { setError(e instanceof Error ? e.message : "Could not confirm milestone."); }
    finally { setBusy(""); }
  };
  return <section className="review"><h2>Compare proposals</h2><p>Select any number of teams, or reject proposals. Decisions are yours.</p>{error && <p role="alert">{error}</p>}
    {!proposals.length && <p>No proposals yet.</p>}
    <div className="review-list">{proposals.map(p => <article className="review-card" key={p.id}>
      <div className="review-heading"><h3>{p.teamName}</h3><span className={`status status-${p.status}`}>{p.status}</span></div>
      {p.tags?.length ? <p>Tags: {p.tags.join(", ")}</p> : null}
      <dl><dt>Idea</dt><dd>{p.idea}</dd><dt>Plan</dt><dd>{p.plan}</dd><dt>Timeline</dt><dd>{p.timeline}</dd></dl>
      {p.prototypeUrl && <a href={p.prototypeUrl} target="_blank" rel="noreferrer">Open prototype</a>}
      <label>Decision reason (optional)<input value={reason[p.id] ?? p.reason ?? ""} onChange={e => setReason(old => ({ ...old, [p.id]: e.target.value }))} /></label>
      <div className="review-actions"><button disabled={busy === p.id || p.status === "selected"} onClick={() => decide(p.id, "selected")}>Select</button><button disabled={busy === p.id || p.status === "rejected"} onClick={() => decide(p.id, "rejected")}>Reject</button>{p.status !== "pending" && !(p.milestones?.length) && <button disabled={busy === p.id} onClick={() => decide(p.id, "pending")}>Reset</button>}</div>
      {p.status === "selected" && <div className="milestone-form"><label>Confirmed milestone title<input value={milestone[p.id] ?? ""} onChange={e => setMilestone(old => ({ ...old, [p.id]: e.target.value }))} placeholder="e.g. Prototype reviewed" /></label><button disabled={busy === p.id} onClick={() => confirmMilestone(p.id)}>Confirm milestone (+10 points)</button></div>}
      {!!p.milestones?.length && <p>Confirmed milestones: {p.milestones.map(m => m.title).join(", ")}</p>}
    </article>)}</div>
  </section>;
}
