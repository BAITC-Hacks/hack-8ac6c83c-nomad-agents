import { useCallback, useEffect, useState } from "react";
import { AppShell } from "./components/AppShell";
import { RoleSwitcher } from "./components/RoleSwitcher";
import { Toaster } from "./components/Toaster";
import { useActor } from "./lib/actor";
import { apiFetch, DEMO_EVENT, isDemoMode } from "./lib/http";
import NewTaskWizard from "./pages/business/NewTaskWizard";
import MyTasks from "./pages/business/MyTasks";
import TaskDetail from "./pages/business/TaskDetail";
import Catalog from "./pages/Catalog";
import TaskView from "./pages/team/TaskView";
import ProposalForm from "./components/ProposalForm";
import ProposalReview from "./components/ProposalReview";
import MyProposals from "./pages/team/MyProposals";
import Leaderboard from "./pages/Leaderboard";
import AiLogs from "./pages/AiLogs";

export interface ActorsDto {
  businesses: { id: string; name: string; industry?: string; contactName?: string }[];
  teams: { id: string; name: string }[];
}
function usePath() {
  const [path, setPath] = useState(location.pathname);
  useEffect(() => { const listener = () => setPath(location.pathname); addEventListener("popstate", listener); return () => removeEventListener("popstate", listener); }, []);
  const navigate = useCallback((next: string) => { if (next !== location.pathname) history.pushState({}, "", next); setPath(next); }, []);
  return [path, navigate] as const;
}
export default function App() {
  const { actor } = useActor();
  const [actors, setActors] = useState<ActorsDto | null>(null);
  const [error, setError] = useState("");
  const [path, navigate] = usePath();
  const [demo, setDemo] = useState(isDemoMode());
  useEffect(() => { const update = () => setDemo(isDemoMode()); addEventListener(DEMO_EVENT, update); return () => removeEventListener(DEMO_EVENT, update); }, []);
  useEffect(() => { apiFetch<ActorsDto>("/api/actors").then(setActors).catch(() => setError("Profiles could not be loaded.")); }, []);
  useEffect(() => {
    if (!actor) return;
    if (path === "/" || (actor.role === "business" && (path.startsWith("/team/") || path.startsWith("/catalog/"))) || (actor.role === "team" && path.startsWith("/business/"))) navigate(actor.role === "business" ? "/business/tasks" : "/catalog");
  }, [actor, path, navigate]);
  const ownerId = /^\/business\/tasks\/([^/]+)$/.exec(path)?.[1];
  const wizardId = /^\/business\/tasks\/([^/]+)\/wizard$/.exec(path)?.[1];
  const catalogId = /^\/catalog\/([^/]+)$/.exec(path)?.[1];
  let page = <section className="welcome-card"><span className="eyebrow">WELCOME TO TASKFORGE</span><h1>Choose who you are</h1><p>Select a business or team to open the workspace.</p></section>;
  if (actor) {
    if (path === "/business/tasks") page = <MyTasks />;
    else if (path === "/business/tasks/new" || wizardId) page = <NewTaskWizard taskId={wizardId} onCreated={id => navigate(`/business/tasks/${id}/wizard`)} />;
    else if (ownerId) page = <TaskDetail key={actor.actorId} taskId={ownerId} renderProposals={task => <ProposalReview taskId={task.id} />} />;
    else if (path === "/catalog") page = <Catalog />;
    else if (catalogId) page = <TaskView key={actor.actorId} taskId={catalogId} renderProposalForm={({ task, proposal, readOnly }) => <ProposalForm key={proposal?.id ?? `${actor.actorId}:new`} taskId={task.id} proposal={proposal} readOnly={readOnly} />} />;
    else if (path === "/team/proposals") page = <MyProposals />;
    else if (path === "/leaderboard") page = <Leaderboard />;
    else if (path === "/ai-logs") page = <AiLogs />;
    else page = <section className="page-card"><h1>Page not found</h1><a href={actor.role === "business" ? "/business/tasks" : "/catalog"}>Return to workspace</a></section>;
  }
  return <><AppShell actor={actor} path={path} navigate={navigate} roleSwitcher={<RoleSwitcher actors={actors} error={error} />}>
    {demo && <div className="demo-banner" role="status"><strong>Sample workspace</strong> · API unavailable. Data and sample scores live only in this tab and reset on reload; they are not official ratings.</div>}
    {page}
  </AppShell><Toaster /></>;
}
