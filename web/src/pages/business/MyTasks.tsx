import { useEffect, useState } from "react";
import { LevelBadge } from "../../components/LevelBadge";
import { useActor } from "../../lib/actor";
import { ApiError, apiFetch } from "../../lib/http";
import type { CatalogTask } from "../types";

interface MyTasksResponse { items: CatalogTask[] }

export default function MyTasks() {
  const { actor } = useActor();
  const [tasks, setTasks] = useState<CatalogTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    setLoading(true);
    apiFetch<MyTasksResponse | CatalogTask[]>("/api/tasks/mine")
      .then((data) => { if (active) setTasks(Array.isArray(data) ? data : data.items ?? []); })
      .catch((err: unknown) => { if (active) setError(err instanceof ApiError ? err.message : "Could not load your tasks."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [actor?.actorId]);

  return (
    <main style={{ maxWidth: 1040, margin: "0 auto", padding: "2rem 1.25rem", color: "#172033", fontFamily: "system-ui, sans-serif" }}>
      <header style={{ display: "flex", justifyContent: "space-between", gap: 16, alignItems: "center", flexWrap: "wrap", marginBottom: "1.5rem" }}>
        <div><p style={{ color: "#64748b", margin: "0 0 0.3rem" }}>Business workspace</p><h1 style={{ margin: 0 }}>My tasks</h1><p style={{ color: "#64748b" }}>Track each task’s status, readiness, and place in the shared catalog.</p></div>
        <a href="/business/tasks/new" style={{ background: "#1d4ed8", color: "white", textDecoration: "none", padding: "0.65rem 0.9rem", borderRadius: 8, fontWeight: 700 }}>New task</a>
      </header>
      {loading && <p>Loading your tasks…</p>}
      {error && <p role="alert" style={{ color: "#b91c1c" }}>{error}</p>}
      {!loading && !error && tasks.length === 0 && <section style={{ border: "1px dashed #cbd5e1", borderRadius: 12, padding: "2rem", textAlign: "center" }}><h2>No tasks yet</h2><p>Create a task to describe a challenge for student teams.</p><a href="/business/tasks/new">Create your first task</a></section>}
      {!loading && tasks.length > 0 && <div style={{ overflowX: "auto", border: "1px solid #e2e8f0", borderRadius: 12, background: "white" }}>
        <table style={{ borderCollapse: "collapse", width: "100%", minWidth: 690 }}>
          <thead><tr style={{ textAlign: "left", background: "#f8fafc" }}>{["Task", "Status", "Readiness", "Catalog position", "Proposals", ""].map((label) => <th key={label} style={{ padding: "0.8rem", borderBottom: "1px solid #e2e8f0" }}>{label}</th>)}</tr></thead>
          <tbody>{tasks.map((task) => {
            const position = task.position ?? task.catalogPosition;
            const total = task.catalogTotal;
            return <tr key={task.id} style={{ borderBottom: "1px solid #f1f5f9" }}>
              <td style={{ padding: "0.8rem", fontWeight: 700 }}>{task.title}</td>
              <td style={{ padding: "0.8rem" }}><span style={{ textTransform: "capitalize" }}>{task.status ?? "editing"}</span>{task.hasUnconfirmedChanges && <span title="Changes are not confirmed" style={{ marginLeft: 6, color: "#b45309" }}>● Unconfirmed changes</span>}</td>
              <td style={{ padding: "0.8rem" }}>{task.rating ? <span style={{ display: "inline-flex", alignItems: "center", gap: 8 }}><strong>{task.rating.total}/100</strong><LevelBadge level={task.rating.level} /></span> : <span style={{ color: "#64748b" }}>Not scored</span>}</td>
              <td style={{ padding: "0.8rem" }}>{position != null ? `#${position}${total != null ? ` of ${total}` : ""}` : "—"}</td>
              <td style={{ padding: "0.8rem" }}>{task.proposalCount}</td>
              <td style={{ padding: "0.8rem" }}><a href={`/business/tasks/${encodeURIComponent(task.id)}`}>Open</a></td>
            </tr>;
          })}</tbody>
        </table>
      </div>}
    </main>
  );
}
