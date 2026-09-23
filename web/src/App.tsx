import { useEffect, useState } from "react";
import { apiFetch, ApiError } from "./lib/http";
import { useActor } from "./lib/actor";

interface HealthDto {
  status: string;
  aiMode: string;
  firestore: string;
}

interface BusinessDto {
  id: string;
  name: string;
  industry: string;
  contactName: string;
}

interface TeamDto {
  id: string;
  name: string;
}

interface ActorsDto {
  businesses: BusinessDto[];
  teams: TeamDto[];
}

export default function App() {
  const { actor, setActor } = useActor();
  const [health, setHealth] = useState<HealthDto | null>(null);
  const [actors, setActors] = useState<ActorsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiFetch<HealthDto>("/api/health")
      .then(setHealth)
      .catch((err: unknown) => setError(err instanceof ApiError ? err.message : "API unreachable"));

    apiFetch<ActorsDto>("/api/actors")
      .then(setActors)
      .catch((err: unknown) => setError(err instanceof ApiError ? err.message : "API unreachable"));
  }, []);

  return (
    <main style={{ fontFamily: "sans-serif", padding: "2rem", maxWidth: 640, margin: "0 auto" }}>
      <h1>TaskForge</h1>
      <p>AI Challenge Coach</p>

      {error && <p style={{ color: "crimson" }}>Error: {error}</p>}

      <section>
        <h2>API health</h2>
        <pre>{health ? JSON.stringify(health, null, 2) : "loading..."}</pre>
      </section>

      <section>
        <h2>Actor</h2>
        <select
          value={actor ? `${actor.role}:${actor.actorId}` : ""}
          onChange={(e) => {
            const [role, actorId] = e.target.value.split(":");
            if (!role || !actorId) {
              setActor(null);
              return;
            }
            setActor({ role: role as "business" | "team", actorId });
          }}
        >
          <option value="">Select actor…</option>
          <optgroup label="Businesses">
            {actors?.businesses.map((b) => (
              <option key={b.id} value={`business:${b.id}`}>
                {b.name}
              </option>
            ))}
          </optgroup>
          <optgroup label="Teams">
            {actors?.teams.map((t) => (
              <option key={t.id} value={`team:${t.id}`}>
                {t.name}
              </option>
            ))}
          </optgroup>
        </select>
        {actor && (
          <p>
            Acting as <strong>{actor.role}</strong> / <code>{actor.actorId}</code>
          </p>
        )}
      </section>
    </main>
  );
}
