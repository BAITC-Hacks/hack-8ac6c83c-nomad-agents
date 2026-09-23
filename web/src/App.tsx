import { useCallback, useEffect, useMemo, useState } from "react";
import { AppShell } from "./components/AppShell";
import { RoleSwitcher } from "./components/RoleSwitcher";
import { useActor } from "./lib/actor";
import { apiFetch } from "./lib/http";
import { Toaster } from "./components/Toaster";

export interface ActorsDto {
  businesses: { id: string; name: string; industry?: string; contactName?: string }[];
  teams: { id: string; name: string }[];
}

const routes = {
  "/business/tasks": { title: "My tasks", description: "Your business tasks will appear here." },
  "/business/tasks/new": { title: "New task", description: "Start a task draft to work with the AI Challenge Coach." },
  "/catalog": { title: "Task catalog", description: "Browse published tasks and their readiness scores." },
  "/team/proposals": { title: "My proposals", description: "Track the proposals submitted by your team." },
} as const;

function resolvePage(path: string) {
  const fixed = routes[path as keyof typeof routes];
  if (fixed) return fixed;
  if (/^\/business\/tasks\/[^/]+(?:\/wizard)?$/.test(path)) return { title: "Task details", description: "Review the confirmed task card, readiness score, and team proposals." };
  if (/^\/catalog\/[^/]+$/.test(path)) return { title: "Task details", description: "Review this published task and prepare a proposal." };
  if (path === "/leaderboard") return { title: "Leaderboard", description: "Team points and standings will appear here." };
  if (path === "/ai-logs") return { title: "AI logs", description: "Analyze requests and validation results will appear here." };
  return undefined;
}

function usePath() {
  const [path, setPath] = useState(() => window.location.pathname);
  useEffect(() => {
    const onPopState = () => setPath(window.location.pathname);
    window.addEventListener("popstate", onPopState);
    return () => window.removeEventListener("popstate", onPopState);
  }, []);
  const navigate = useCallback((next: string) => {
    if (next !== window.location.pathname) window.history.pushState({}, "", next);
    setPath(next);
  }, []);
  return [path, navigate] as const;
}

export default function App() {
  const { actor } = useActor();
  const [actors, setActors] = useState<ActorsDto | null>(null);
  const [actorsError, setActorsError] = useState("");
  const [path, navigate] = usePath();

  useEffect(() => {
    apiFetch<ActorsDto>("/api/actors")
      .then((data) => { setActors(data); setActorsError(""); })
      .catch(() => setActorsError("Could not load businesses and teams. Check that the API is running, then reload."));
  }, []);

  const activePath = useMemo(() => {
    const pageAtPath = resolvePage(path);
    const isBusinessRoute = path.startsWith("/business/");
    if (pageAtPath && actor && isBusinessRoute === (actor.role === "business")) return path;
    if (!actor && pageAtPath) return path;
    return actor?.role === "business" ? "/business/tasks" : "/catalog";
  }, [actor, path]);
  const page = resolvePage(activePath);

  useEffect(() => {
    if (resolvePage(path) !== undefined && actor && path !== activePath) navigate(activePath);
    else if (!resolvePage(path) && actor) navigate(activePath);
  }, [actor, activePath, navigate, path]);

  return <>
    <AppShell actor={actor} path={activePath} navigate={navigate} roleSwitcher={<RoleSwitcher actors={actors} error={actorsError} />}>
      {!actor ? (
        <section className="welcome-card">
          <span className="eyebrow">WELCOME TO TASKFORGE</span>
          <h1>Choose who you are</h1>
          <p>Select a business or student team to open your workspace.</p>
        </section>
      ) : page ? (
        <section className="page-card">
          <span className="eyebrow">{actor.role === "business" ? "BUSINESS WORKSPACE" : "TEAM WORKSPACE"}</span>
          <h1>{page.title}</h1>
          <p>{page.description}</p>
          <div className="integration-note">This page is ready for its feature module to be connected.</div>
        </section>
      ) : null}
    </AppShell>
    <Toaster />
  </>;
}
