import { useEffect, useMemo, useState } from "react";
import { LevelBadge } from "../components/LevelBadge";
import { RatingPanel } from "../components/RatingPanel";
import { useActor } from "../lib/actor";
import { ApiError, apiFetch } from "../lib/http";
import { type CatalogResponse, type CatalogTask, type ReadinessLevel, taskSummary } from "./types";

const levels: { value: ReadinessLevel; label: string }[] = [
  { value: "draft", label: "Needs clarification" },
  { value: "workable", label: "Workable" },
  { value: "ready", label: "Ready" },
  { value: "priority", label: "Priority" },
];

interface Recommendation { task: CatalogTask; matchedTags?: string[]; matched?: string[] }

function normalizeCatalog(data: CatalogResponse | CatalogTask[]): CatalogResponse {
  return Array.isArray(data) ? { items: data, total: data.length } : data;
}

function normalizeRecommendations(data: Recommendation[] | { items?: Recommendation[] }): Recommendation[] {
  return Array.isArray(data) ? data : data.items ?? [];
}

export default function Catalog() {
  const { actor } = useActor();
  const [topics, setTopics] = useState<string[]>([]);
  const [selectedTopics, setSelectedTopics] = useState<string[]>([]);
  const [selectedLevels, setSelectedLevels] = useState<ReadinessLevel[]>([]);
  const [catalog, setCatalog] = useState<CatalogResponse | null>(null);
  const [recommendations, setRecommendations] = useState<Recommendation[]>([]);
  const [teamName, setTeamName] = useState("your team");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiFetch<string[] | { topics: string[] }>("/api/catalog/topics")
      .then((data) => setTopics(Array.isArray(data) ? data : data.topics ?? []))
      .catch(() => setTopics([]));
  }, []);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(null);
    const query = new URLSearchParams();
    selectedTopics.forEach((topic) => query.append("topic", topic));
    selectedLevels.forEach((level) => query.append("level", level));
    apiFetch<CatalogResponse | CatalogTask[]>(`/api/catalog${query.size ? `?${query}` : ""}`)
      .then((data) => { if (active) setCatalog(normalizeCatalog(data)); })
      .catch((err: unknown) => { if (active) setError(err instanceof ApiError ? err.message : "Could not load the catalog."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [selectedTopics, selectedLevels]);

  useEffect(() => {
    if (actor?.role !== "team" || !actor.actorId) {
      setRecommendations([]);
      return;
    }
    let active = true;
    apiFetch<Recommendation[] | { items?: Recommendation[]; teamName?: string }>(`/api/teams/${encodeURIComponent(actor.actorId)}/recommendations`)
      .then((data) => {
        if (!active) return;
        setRecommendations(normalizeRecommendations(data).slice(0, 3));
        if (!Array.isArray(data) && data.teamName) setTeamName(data.teamName);
      })
      .catch(() => { if (active) setRecommendations([]); });
    return () => { active = false; };
  }, [actor]);

  const recommendationTasks = useMemo(() => recommendations.slice(0, 3), [recommendations]);

  const toggleTopic = (topic: string) => setSelectedTopics((selected) => selected.includes(topic) ? selected.filter((value) => value !== topic) : [...selected, topic]);
  const toggleLevel = (level: ReadinessLevel) => setSelectedLevels((selected) => selected.includes(level) ? selected.filter((value) => value !== level) : [...selected, level]);

  return (
    <main style={{ maxWidth: 1040, margin: "0 auto", padding: "2rem 1.25rem", color: "#172033", fontFamily: "system-ui, sans-serif" }}>
      <header style={{ marginBottom: "1.5rem" }}>
        <p style={{ color: "#64748b", margin: "0 0 0.3rem" }}>TaskForge · Shared catalog</p>
        <h1 style={{ margin: 0 }}>Find a challenge</h1>
        <p style={{ color: "#64748b" }}>Explore confirmed business tasks. Every team can propose to any published challenge.</p>
      </header>

      {actor?.role === "team" && recommendationTasks.length > 0 && (
        <section aria-label={`Recommended for ${teamName}`} style={{ border: "1px solid #bfdbfe", borderRadius: 14, background: "#eff6ff", padding: "1rem", marginBottom: "1.5rem" }}>
          <h2 style={{ margin: "0 0 0.8rem", fontSize: "1.15rem" }}>Recommended for {teamName}</h2>
          <div style={{ display: "grid", gap: 10, gridTemplateColumns: "repeat(auto-fit, minmax(230px, 1fr))" }}>
            {recommendationTasks.map(({ task, matchedTags, matched }) => <article key={task.id} style={{ padding: "0.8rem", borderRadius: 10, background: "white", border: "1px solid #dbeafe" }}>
              <a href={`/catalog/${encodeURIComponent(task.id)}`} style={{ fontWeight: 700, color: "#1d4ed8" }}>{task.title}</a>
              <p style={{ margin: "0.4rem 0", color: "#475569", fontSize: "0.9rem" }}>{task.businessName}</p>
              <p style={{ margin: 0, color: "#475569", fontSize: "0.85rem" }}>Matches your tags: {(matchedTags ?? matched ?? []).join(", ") || "Related interests"}</p>
            </article>)}
          </div>
        </section>
      )}

      <div style={{ display: "grid", gridTemplateColumns: "minmax(190px, 240px) 1fr", gap: "1.5rem", alignItems: "start" }}>
        <aside style={{ border: "1px solid #e2e8f0", borderRadius: 12, padding: "1rem", background: "#fff" }}>
          <h2 style={{ fontSize: "1rem", margin: "0 0 0.7rem" }}>Filter catalog</h2>
          <fieldset style={{ border: 0, padding: 0, margin: "0 0 1rem" }}>
            <legend style={{ fontWeight: 700, marginBottom: 8 }}>Topic</legend>
            {topics.length ? topics.map((topic) => <label key={topic} style={{ display: "flex", gap: 8, alignItems: "center", padding: "0.2rem 0", fontSize: "0.9rem" }}>
              <input type="checkbox" checked={selectedTopics.includes(topic)} onChange={() => toggleTopic(topic)} />{topic}
            </label>) : <span style={{ color: "#64748b", fontSize: "0.85rem" }}>No topics available</span>}
          </fieldset>
          <fieldset style={{ border: 0, padding: 0, margin: 0 }}>
            <legend style={{ fontWeight: 700, marginBottom: 8 }}>Readiness level</legend>
            {levels.map(({ value, label }) => <label key={value} style={{ display: "flex", gap: 8, alignItems: "center", padding: "0.2rem 0", fontSize: "0.9rem" }}>
              <input type="checkbox" checked={selectedLevels.includes(value)} onChange={() => toggleLevel(value)} />{label}
            </label>)}
          </fieldset>
          {(selectedTopics.length > 0 || selectedLevels.length > 0) && <button type="button" onClick={() => { setSelectedTopics([]); setSelectedLevels([]); }} style={{ marginTop: 12 }}>Clear filters</button>}
        </aside>

        <section>
          <p aria-live="polite" style={{ marginTop: 0, color: "#475569" }}>
            {loading ? "Loading catalog…" : `Showing ${catalog?.items.length ?? 0} of ${catalog?.total ?? 0}`}
          </p>
          {error && <p role="alert" style={{ color: "#b91c1c" }}>{error}</p>}
          {!loading && !error && catalog?.items.length === 0 && <p>No tasks match these filters. Try clearing a filter.</p>}
          <div style={{ display: "grid", gap: 14 }}>
            {catalog?.items.map((task) => {
              const rating = task.rating;
              const rank = task.position ?? task.catalogPosition;
              const total = task.catalogTotal ?? catalog.total;
              const priority = rating?.level === "priority";
              return <article key={task.id} style={{ border: `1px solid ${priority ? "#d4a017" : "#e2e8f0"}`, borderRadius: 14, padding: "1rem", background: priority ? "#fffbeb" : "white", boxShadow: priority ? "0 0 0 1px #fef3c7" : undefined }}>
                <div style={{ display: "flex", flexWrap: "wrap", gap: 10, alignItems: "center" }}>
                  {rank != null && <span style={{ color: "#64748b", fontWeight: 700 }}>#{rank} of {total}</span>}
                  <h2 style={{ fontSize: "1.15rem", margin: 0, flex: 1 }}>{priority && <span aria-label="Priority task" style={{ color: "#a16207", marginRight: 6 }}>★</span>}{task.title}</h2>
                  {rating && <LevelBadge level={rating.level} />}
                </div>
                <p style={{ color: "#475569", margin: "0.55rem 0 0.8rem" }}>{task.businessName}</p>
                <p style={{ lineHeight: 1.5, margin: "0 0 0.8rem" }}>{taskSummary(task)}</p>
                {rating && <RatingPanel rating={rating} variant="compact" position={rank != null ? { rank, total } : null} />}
                <div style={{ display: "flex", flexWrap: "wrap", gap: 6, margin: "0.8rem 0" }}>
                  {task.topics.map((topic) => <span key={`topic-${topic}`} style={{ padding: "0.2rem 0.55rem", borderRadius: 999, background: "#f1f5f9", fontSize: "0.8rem" }}>{topic}</span>)}
                  {task.techTags.map((tag) => <span key={`tech-${tag}`} style={{ padding: "0.2rem 0.55rem", borderRadius: 999, background: "#eef2ff", color: "#3730a3", fontSize: "0.8rem" }}>{tag}</span>)}
                </div>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 12 }}>
                  <span style={{ color: "#64748b", fontSize: "0.9rem" }}>{task.proposalCount} proposal{task.proposalCount === 1 ? "" : "s"}</span>
                  <a href={`/catalog/${encodeURIComponent(task.id)}`} style={{ display: "inline-block", background: "#1d4ed8", color: "white", textDecoration: "none", padding: "0.55rem 0.85rem", borderRadius: 8, fontWeight: 700 }}>View details</a>
                </div>
              </article>;
            })}
          </div>
        </section>
      </div>
    </main>
  );
}
