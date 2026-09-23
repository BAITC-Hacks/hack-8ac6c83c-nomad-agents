import { useEffect, useState, type ReactNode } from "react";
import { LevelBadge, type ReadinessLevel } from "../../components/LevelBadge";
import { RatingPanel, type RatingDto } from "../../components/RatingPanel";
import { apiFetch, ApiError } from "../../lib/http";

export type TaskDetailDto = {
  id: string;
  status: "editing" | "published";
  fields: Record<string, string | string[] | null | undefined>;
  confirmed?: { fields?: Record<string, string | string[] | null | undefined>; confirmedAt?: string } | null;
  rating?: RatingDto | null;
  hasUnconfirmedChanges?: boolean;
  proposalCount?: number;
  position?: { rank: number; total: number } | number | null;
  catalogTotal?: number;
};

export type TaskDetailProps = {
  taskId: string;
  /** F4 can mount its proposal comparison view here without owning this page shell. */
  renderProposals?: (task: TaskDetailDto) => ReactNode;
};

const CARD_FIELDS: { key: string; label: string }[] = [
  { key: "title", label: "Title" },
  { key: "context", label: "Context" },
  { key: "need", label: "Need" },
  { key: "expectedResult", label: "Expected result" },
  { key: "successCriteria", label: "Success criteria" },
  { key: "users", label: "Users" },
  { key: "data", label: "Data and materials" },
  { key: "constraints", label: "Constraints" },
  { key: "contact", label: "Contact" },
  { key: "interactionFormat", label: "Interaction format" },
  { key: "topics", label: "Topics" },
  { key: "techTags", label: "Technologies" },
];

function showValue(value: string | string[] | null | undefined) {
  if (Array.isArray(value)) return value.length ? value.join(", ") : "Not provided";
  return value?.trim() || "Not provided";
}

export default function TaskDetail({ taskId, renderProposals }: TaskDetailProps) {
  const [task, setTask] = useState<TaskDetailDto | null>(null);
  const [activeTab, setActiveTab] = useState<"card" | "rating" | "proposals">("card");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [editable, setEditable] = useState<TaskDetailDto["fields"]>({});
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [delta, setDelta] = useState<number | null>(null);
  const [previousLevel, setPreviousLevel] = useState<ReadinessLevel | null>(null);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(null);
    apiFetch<TaskDetailDto>(`/api/tasks/${encodeURIComponent(taskId)}`)
      .then((result) => { if (active) { setTask(result); setEditable(result.fields); } })
      .catch((err: unknown) => {
        if (!active) return;
        setError(err instanceof ApiError ? err.message : "Could not load this task.");
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [taskId]);

  if (loading) return <main style={{ padding: "2rem" }}><p>Loading task…</p></main>;
  if (!task) return <main style={{ padding: "2rem" }}><h1>Task detail</h1><p role="alert">{error ?? "Task not found."}</p></main>;

  const fields = task.confirmed?.fields ?? task.fields;
  const rating = task.rating;
  const save = async () => {
    setBusy(true); setError(null);
    try {
      const saved = await apiFetch<TaskDetailDto>(`/api/tasks/${encodeURIComponent(taskId)}/fields`, { method: "PUT", body: JSON.stringify({ fields: editable }) });
      setTask(saved); setMessage("Card edits saved. Confirm to update the rating and catalog card.");
    } catch (e) { setError(e instanceof Error ? e.message : "Could not save card."); }
    finally { setBusy(false); }
  };
  const confirm = async () => {
    setBusy(true); setError(null);
    try {
      await apiFetch(`/api/tasks/${encodeURIComponent(taskId)}/fields`, { method: "PUT", body: JSON.stringify({ fields: editable }) });
      const response = await apiFetch<{ task: TaskDetailDto; rating: RatingDto; delta: number }>(`/api/tasks/${encodeURIComponent(taskId)}/confirm`, { method: "POST" });
      setPreviousLevel(rating?.level ?? null); setDelta(response.delta); setTask(response.task); setEditing(false); setMessage("Card confirmed and rating updated."); setActiveTab("rating");
    } catch (e) { setError(e instanceof Error ? e.message : "Could not confirm card."); }
    finally { setBusy(false); }
  };
  const focusField = (key: string) => { setEditing(true); setActiveTab("card"); window.setTimeout(() => document.getElementById(`owner-field-${key}`)?.focus(), 0); };
  const tabs = [
    { id: "card", label: "Card" },
    { id: "rating", label: "Rating" },
    { id: "proposals", label: `Proposals${task.proposalCount === undefined ? "" : ` (${task.proposalCount})`}` },
  ] as const;

  return (
    <main style={{ maxWidth: 1060, margin: "0 auto", padding: "1.5rem" }}>
      <a href="/business/tasks" style={{ color: "#1d4ed8" }}>← My tasks</a>
      <header style={{ display: "flex", alignItems: "center", gap: 12, flexWrap: "wrap", margin: "1rem 0" }}>
        <h1 style={{ margin: 0, flex: "1 1 auto" }}>{showValue(fields.title)}</h1>
        {rating && <LevelBadge level={rating.level as ReadinessLevel} />}
        <span style={{ color: "#64748b" }}>{task.status === "published" ? "Published" : "Editing"}</span>
      </header>

      {task.hasUnconfirmedChanges && <p role="status" style={{ padding: "0.8rem", background: "#fff7ed", color: "#9a3412", borderRadius: 8 }}>Unconfirmed changes — the catalog shows the last confirmed version.</p>}
      {message && <p role="status">{message}</p>}
      {error && <p role="alert">{error}</p>}

      <nav aria-label="Task detail sections" style={{ display: "flex", gap: 8, borderBottom: "1px solid #cbd5e1", marginBottom: "1rem" }}>
        {tabs.map((tab) => <button key={tab.id} type="button" aria-current={activeTab === tab.id ? "page" : undefined} onClick={() => setActiveTab(tab.id)} style={{ border: 0, borderBottom: activeTab === tab.id ? "3px solid #2563eb" : "3px solid transparent", background: "transparent", padding: "0.7rem 0.8rem", cursor: "pointer", fontWeight: activeTab === tab.id ? 700 : 400 }}>{tab.label}</button>)}
      </nav>

      {activeTab === "card" && <section aria-label="Confirmed task card">
        <button type="button" onClick={() => setEditing(value => !value)}>{editing ? "View confirmed card" : "Edit card"}</button>
        {editing && <div className="owner-editor"><p>Changes appear in the catalog only after confirmation.</p>{CARD_FIELDS.map(({ key, label }) => <label key={key} htmlFor={`owner-field-${key}`}>{label}{key === "topics" || key === "techTags" ? <input id={`owner-field-${key}`} value={Array.isArray(editable[key]) ? editable[key].join(", ") : ""} onChange={event => setEditable(old => ({ ...old, [key]: event.target.value.split(",").map(value => value.trim()).filter(Boolean) }))} /> : <textarea id={`owner-field-${key}`} rows={key === "title" ? 1 : 3} value={String(editable[key] ?? "")} onChange={event => setEditable(old => ({ ...old, [key]: event.target.value }))} />}</label>)}<div className="review-actions"><button disabled={busy} onClick={save}>Save edits</button><button disabled={busy || !String(editable.title ?? "").trim()} onClick={confirm}>Confirm & re-score</button></div></div>}
        {task.confirmed?.confirmedAt && <p style={{ color: "#64748b" }}>Last confirmed {new Date(task.confirmed.confirmedAt).toLocaleString()}</p>}
        <dl style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "0.8rem 1.5rem" }}>
          {CARD_FIELDS.filter(({ key }) => key in fields).map(({ key, label }) => <div key={key} style={{ padding: "0.8rem", background: key === "expectedResult" || key === "successCriteria" || key === "constraints" ? "#f8fafc" : "white", border: "1px solid #e2e8f0", borderRadius: 8 }}>
            <dt style={{ fontWeight: 700, marginBottom: 4 }}>{label}</dt><dd style={{ margin: 0, whiteSpace: "pre-wrap" }}>{showValue(fields[key])}</dd>
          </div>)}
        </dl>
      </section>}

      {activeTab === "rating" && (rating ? <RatingPanel rating={rating} variant="full" position={typeof task.position === "number" ? { rank: task.position, total: task.catalogTotal ?? 0 } : task.position} delta={delta} previousLevel={previousLevel} onAddDetails={focusField} /> : <p>This task has not been scored yet.</p>)}

      {activeTab === "proposals" && (renderProposals ? renderProposals(task) : <section aria-label="Proposals"><h2>Proposals</h2><p>The proposal review panel will appear here.</p></section>)}
    </main>
  );
}
