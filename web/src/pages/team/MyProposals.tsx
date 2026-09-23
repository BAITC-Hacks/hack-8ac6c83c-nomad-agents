import { useEffect, useState } from "react";
import { apiFetch } from "../../lib/http";
import { useActor } from "../../lib/actor";

type Proposal = { id: string; taskId: string; taskTitle?: string; status: string; idea: string; timeline: string; milestones?: { id: string; title: string }[] };
export default function MyProposals() {
  const { actor } = useActor();
  const [items, setItems] = useState<Proposal[]>([]);
  const [error, setError] = useState("");
  useEffect(() => { apiFetch<Proposal[]>("/api/proposals/mine").then(data => setItems(Array.isArray(data) ? data : [])).catch(e => setError(e instanceof Error ? e.message : "Could not load proposals.")); }, [actor?.actorId]);
  return <section className="content-page"><p className="eyebrow">TEAM WORKSPACE</p><h1>My proposals</h1>{error && <p role="alert">{error}</p>}
    {!error && !items.length && <p>No proposals submitted yet. <a href="/catalog">Browse the catalog</a> to find a task.</p>}
    <div className="proposal-list">{items.map(p => <article className="review-card" key={p.id}><div className="review-heading"><h2><a href={`/catalog/${p.taskId}`}>{p.taskTitle || p.taskId}</a></h2><span className={`status status-${p.status}`}>{p.status}</span></div><p>{p.idea}</p><p>Timeline: {p.timeline}</p>{!!p.milestones?.length && <p>Milestones: {p.milestones.map(m => m.title).join(", ")}</p>}</article>)}</div>
  </section>;
}
