import { useEffect, useState } from "react";
import { apiFetch } from "../lib/http";

type Log = { id: string; createdAt?: string; kind?: string; taskId?: string; model?: string; validation?: string; latencyMs?: number; systemPrompt?: string; input?: unknown; rawOutput?: unknown; errors?: string[] };
function pretty(value: unknown) { if (typeof value === "string") { try { return JSON.stringify(JSON.parse(value), null, 2); } catch { return value; } } return JSON.stringify(value, null, 2) ?? "—"; }
export default function AiLogs() {
  const [logs, setLogs] = useState<Log[]>([]);
  const [filter, setFilter] = useState("");
  const [error, setError] = useState("");
  useEffect(() => { apiFetch<Log[] | { items: Log[] }>(`/api/ai-logs${filter ? `?taskId=${encodeURIComponent(filter)}` : ""}`).then(data => setLogs(Array.isArray(data) ? data : data.items ?? [])).catch(e => setError(e instanceof Error ? e.message : "Could not load AI logs.")); }, [filter]);
  return <section className="content-page"><p className="eyebrow">AI CHALLENGE COACH</p><h1>AI logs</h1><p>Analysis attempts, validation, and fallback details.</p><label>Filter by task ID <input value={filter} onChange={e => setFilter(e.target.value)} /></label>{error && <p role="alert">{error}</p>}
    {!logs.length && !error && <p>No AI attempts recorded yet.</p>}
    <div className="log-list">{logs.map(log => <details className="review-card" key={log.id}><summary>{log.createdAt ? new Date(log.createdAt).toLocaleString() : "—"} · {log.kind ?? "analysis"} · {log.taskId ?? "—"} · {log.model ?? "—"} · {log.validation ?? "—"} · {log.latencyMs ?? 0} ms</summary><h3>System prompt</h3><pre>{pretty(log.systemPrompt)}</pre><h3>Input</h3><pre>{pretty(log.input)}</pre><h3>Raw output</h3><pre>{pretty(log.rawOutput)}</pre>{!!log.errors?.length && <><h3>Validation errors</h3><ul>{log.errors.map((e, i) => <li key={i}>{e}</li>)}</ul></>}</details>)}</div>
  </section>;
}
