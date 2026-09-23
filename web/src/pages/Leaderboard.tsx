import { useCallback, useEffect, useState } from "react";
import { ApiError, apiFetch } from "../lib/http";

interface LeaderboardTeam {
  teamId: string;
  name: string;
  points: number;
}

const cardStyle: React.CSSProperties = {
  background: "#fff",
  border: "1px solid #e5e7eb",
  borderRadius: 16,
  boxShadow: "0 12px 32px rgba(15, 23, 42, 0.06)",
};

export default function Leaderboard() {
  const [teams, setTeams] = useState<LeaderboardTeam[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      // The API owns leaderboard ordering; preserve its order in the UI.
      const result = await apiFetch<LeaderboardTeam[]>("/api/leaderboard");
      setTeams(result);
    } catch (err: unknown) {
      setError(err instanceof ApiError ? err.message : "Could not reach the leaderboard.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  return (
    <main
      style={{
        minHeight: "100vh",
        background: "#f8fafc",
        color: "#0f172a",
        fontFamily: "Inter, ui-sans-serif, system-ui, sans-serif",
        padding: "clamp(1.25rem, 5vw, 4rem) 1rem",
      }}
    >
      <div style={{ maxWidth: 760, margin: "0 auto" }}>
        <header style={{ marginBottom: 28 }}>
          <p style={{ color: "#64748b", fontSize: 12, fontWeight: 700, letterSpacing: ".14em", textTransform: "uppercase", margin: "0 0 8px" }}>
            TaskForge · Team progress
          </p>
          <div style={{ display: "flex", alignItems: "flex-end", justifyContent: "space-between", gap: 16 }}>
            <div>
              <h1 style={{ fontSize: "clamp(2rem, 6vw, 3rem)", letterSpacing: "-.04em", lineHeight: 1.05, margin: 0 }}>
                Leaderboard
              </h1>
              <p style={{ color: "#64748b", margin: "10px 0 0" }}>
                Confirmed milestones earn points for the team.
              </p>
            </div>
            <button
              type="button"
              onClick={() => void refresh()}
              disabled={loading}
              style={{
                border: "1px solid #cbd5e1",
                borderRadius: 10,
                background: "white",
                color: "#0f172a",
                cursor: loading ? "wait" : "pointer",
                font: "inherit",
                fontSize: 14,
                fontWeight: 650,
                padding: "10px 14px",
                whiteSpace: "nowrap",
              }}
            >
              {loading ? "Refreshing…" : "Refresh"}
            </button>
          </div>
        </header>

        <section aria-label="Team leaderboard" style={cardStyle}>
          <div style={{ display: "grid", gridTemplateColumns: "64px 1fr 100px", gap: 12, padding: "14px 20px", borderBottom: "1px solid #e5e7eb", color: "#64748b", fontSize: 11, fontWeight: 700, letterSpacing: ".1em", textTransform: "uppercase" }}>
            <span>Rank</span>
            <span>Team</span>
            <span style={{ textAlign: "right" }}>Points</span>
          </div>

          {error && (
            <div role="alert" style={{ margin: 16, borderRadius: 10, background: "#fef2f2", color: "#b91c1c", padding: 14, fontSize: 14 }}>
              {error} {teams.length > 0 && "Showing the last loaded standings."}
            </div>
          )}

          {loading && teams.length === 0 ? (
            <p aria-live="polite" style={{ color: "#64748b", padding: "24px 20px", margin: 0 }}>Loading standings…</p>
          ) : error && teams.length === 0 ? (
            <p style={{ color: "#64748b", padding: "24px 20px", margin: 0 }}>Standings could not be loaded. Refresh to try again.</p>
          ) : !loading && !error && teams.length === 0 ? (
            <div style={{ padding: "36px 20px", textAlign: "center" }}>
              <p style={{ fontWeight: 700, margin: "0 0 6px" }}>No teams on the leaderboard yet</p>
              <p style={{ color: "#64748b", margin: 0, fontSize: 14 }}>Team points will appear here after a milestone is confirmed.</p>
            </div>
          ) : (
            teams.map((team, index) => (
              <div
                key={team.teamId}
                style={{ display: "grid", gridTemplateColumns: "64px 1fr 100px", alignItems: "center", gap: 12, padding: "17px 20px", borderBottom: index === teams.length - 1 ? 0 : "1px solid #f1f5f9" }}
              >
                <span style={{ color: index === 0 ? "#b45309" : "#64748b", fontSize: 13, fontWeight: 750 }}>#{index + 1}</span>
                <span style={{ fontWeight: 650 }}>{team.name}</span>
                <span style={{ textAlign: "right", fontVariantNumeric: "tabular-nums", fontWeight: 750 }}>
                  {team.points} <span style={{ color: "#64748b", fontSize: 12, fontWeight: 500 }}>pts</span>
                </span>
              </div>
            ))
          )}
        </section>
        <p style={{ color: "#94a3b8", fontSize: 12, margin: "14px 4px" }}>
          Standings follow the points order provided by TaskForge.
        </p>
      </div>
    </main>
  );
}
