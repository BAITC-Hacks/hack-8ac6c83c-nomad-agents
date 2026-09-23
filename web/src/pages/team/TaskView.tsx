import { useEffect, useState, type ReactNode } from "react";
import { RatingPanel } from "../../components/RatingPanel";
import { useActor } from "../../lib/actor";
import { ApiError, apiFetch } from "../../lib/http";
import { confirmedFields, type CatalogTask, type ProposalSummary } from "../types";

export interface TaskViewProposalMount {
  task: CatalogTask;
  proposal: ProposalSummary | null;
  readOnly: boolean;
}

export interface TaskViewProps {
  taskId?: string;
  /** Lets the application mount F4's ProposalForm without coupling this page to its implementation. */
  renderProposalForm?: (mount: TaskViewProposalMount) => ReactNode;
}

function isProposal(data: unknown): data is ProposalSummary {
  return !!data && typeof data === "object" && "status" in data && "idea" in data;
}

export default function TaskView({ taskId: taskIdProp, renderProposalForm }: TaskViewProps) {
  const { actor } = useActor();
  const [task, setTask] = useState<CatalogTask | null>(null);
  const [proposal, setProposal] = useState<ProposalSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const taskId = taskIdProp ?? decodeURIComponent(window.location.pathname.split("/").filter(Boolean).pop() ?? "");

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(null);
    apiFetch<CatalogTask>(`/api/catalog/${encodeURIComponent(taskId)}`)
      .then((data) => { if (active) setTask(data); })
      .catch((err: unknown) => { if (active) setError(err instanceof ApiError ? err.message : "Could not load this task."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [taskId]);

  useEffect(() => {
    if (!taskId || actor?.role !== "team") {
      setProposal(null);
      return;
    }
    let active = true;
    apiFetch<ProposalSummary | null>(`/api/tasks/${encodeURIComponent(taskId)}/proposals/mine`)
      .then((data) => { if (active) setProposal(data && isProposal(data) ? data : null); })
      .catch((err: unknown) => {
        // A missing proposal is normal; its empty state is presented by the form mount.
        if (active && err instanceof ApiError && err.status !== 404) setError(err.message);
      });
    return () => { active = false; };
  }, [taskId, actor?.role, actor?.actorId]);

  if (loading) return <main style={{ maxWidth: 920, margin: "0 auto", padding: "2rem 1.25rem" }}>Loading task…</main>;
  if (error || !task) return <main style={{ maxWidth: 920, margin: "0 auto", padding: "2rem 1.25rem", fontFamily: "system-ui, sans-serif" }}><p role="alert" style={{ color: "#b91c1c" }}>{error ?? "Task not found."}</p><a href="/catalog">Back to catalog</a></main>;

  const fields = confirmedFields(task);
  const locked = proposal != null && proposal.status !== "pending";
  const sections = [
    ["Context", fields.context],
    ["Need", fields.need],
    ["Users", fields.users],
    ["Data & materials", fields.data],
    ["Expected result", fields.expectedResult],
    ["Success criteria", fields.successCriteria],
    ["Constraints", fields.constraints],
    ["Interaction format", fields.interactionFormat],
    ["Contact", fields.contact],
  ] as const;

  return (
    <main style={{ maxWidth: 1040, margin: "0 auto", padding: "2rem 1.25rem", color: "#172033", fontFamily: "system-ui, sans-serif" }}>
      <a href="/catalog" style={{ color: "#1d4ed8" }}>← Back to catalog</a>
      <header style={{ margin: "1rem 0 1.5rem" }}><p style={{ color: "#64748b", margin: "0 0 0.3rem" }}>{task.businessName}</p><h1 style={{ margin: 0 }}>{fields.title || task.title}</h1></header>
      {task.hasUnconfirmedChanges && <p role="status" style={{ background: "#fffbeb", color: "#92400e", padding: "0.7rem", borderRadius: 8 }}>The business has unconfirmed changes. This view shows the last confirmed card.</p>}
      <div style={{ display: "grid", gridTemplateColumns: "minmax(0, 1.7fr) minmax(270px, 1fr)", gap: "1.25rem", alignItems: "start" }}>
        <article style={{ border: "1px solid #e2e8f0", borderRadius: 14, padding: "1.2rem", background: "white" }}>
          <h2 style={{ marginTop: 0 }}>Confirmed task card</h2>
          {["Expected result", "Success criteria", "Constraints"].map((name) => {
            const value = sections.find(([label]) => label === name)?.[1];
            return value ? <section key={name} style={{ borderLeft: "3px solid #2563eb", paddingLeft: 12, margin: "1rem 0" }}><h3 style={{ margin: "0 0 0.35rem" }}>{name}</h3><p style={{ margin: 0, lineHeight: 1.55 }}>{value}</p></section> : null;
          })}
          {sections.filter(([name]) => !["Expected result", "Success criteria", "Constraints"].includes(name)).map(([name, value]) => value ? <section key={name} style={{ margin: "1rem 0" }}><h3 style={{ fontSize: "1rem", margin: "0 0 0.35rem" }}>{name}</h3><p style={{ margin: 0, lineHeight: 1.55 }}>{value}</p></section> : null)}
          <div style={{ display: "flex", gap: 7, flexWrap: "wrap", marginTop: "1rem" }}>{task.topics.map((tag) => <span key={`topic-${tag}`} style={{ padding: "0.2rem 0.55rem", background: "#f1f5f9", borderRadius: 999, fontSize: "0.82rem" }}>{tag}</span>)}{task.techTags.map((tag) => <span key={`tech-${tag}`} style={{ padding: "0.2rem 0.55rem", background: "#eef2ff", color: "#3730a3", borderRadius: 999, fontSize: "0.82rem" }}>{tag}</span>)}</div>
        </article>
        <aside>
          {task.rating && <RatingPanel rating={task.rating} variant="compact" position={task.position != null ? { rank: task.position, total: task.catalogTotal ?? 0 } : null} />}
          {actor?.role === "team" && <section style={{ marginTop: "1rem", border: "1px solid #e2e8f0", borderRadius: 12, padding: "1rem", background: "white" }}>
            <h2 style={{ margin: "0 0 0.7rem", fontSize: "1.1rem" }}>Your proposal</h2>
            {proposal && <p role="status" style={{ padding: "0.55rem 0.7rem", margin: "0 0 0.8rem", borderRadius: 8, background: proposal.status === "selected" ? "#dcfce7" : proposal.status === "rejected" ? "#fee2e2" : "#f1f5f9" }}>
              Status: <strong style={{ textTransform: "capitalize" }}>{proposal.status}</strong>{locked && " · Proposal edits are locked after a decision."}
            </p>}
            {renderProposalForm ? renderProposalForm({ task, proposal, readOnly: locked }) : <p style={{ color: "#64748b" }}>Proposal form will be available here.</p>}
          </section>}
        </aside>
      </div>
    </main>
  );
}
