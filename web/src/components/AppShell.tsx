import type { ReactNode } from "react";
import type { Actor } from "../lib/actor";

const links = {
  business: [{ path: "/business/tasks", label: "My tasks" }, { path: "/business/tasks/new", label: "New task" }, { path: "/catalog", label: "Catalog" }, { path: "/ai-logs", label: "AI logs" }, { path: "/leaderboard", label: "Leaderboard" }],
  team: [{ path: "/catalog", label: "Catalog" }, { path: "/team/proposals", label: "My proposals" }, { path: "/leaderboard", label: "Leaderboard" }, { path: "/ai-logs", label: "AI logs" }],
};

export function AppShell({ actor, path, navigate, roleSwitcher, children }: {
  actor: Actor | null;
  path: string;
  navigate: (path: string) => void;
  roleSwitcher: ReactNode;
  children: ReactNode;
}) {
  const navLinks = actor ? links[actor.role] : [];
  return <div className="app-frame" onClickCapture={event => {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const anchor = target.closest("a[href]") as HTMLAnchorElement | null;
    if (!anchor || event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey || anchor.target && anchor.target !== "_self") return;
    const destination = new URL(anchor.href, window.location.href);
    if (destination.origin !== window.location.origin) return;
    event.preventDefault();
    navigate(`${destination.pathname}${destination.search}${destination.hash}`);
  }}>
    <header className="topbar">
      <a href={actor?.role === "business" ? "/business/tasks" : "/catalog"} className="brand" onClick={(event) => {
        event.preventDefault(); navigate(actor?.role === "business" ? "/business/tasks" : "/catalog");
      }}>
        <span className="brand-mark">T</span>
        <span><strong>TaskForge</strong><small>AI Challenge Coach</small></span>
      </a>
      <div className="topbar-right">{roleSwitcher}</div>
    </header>
    <div className="workspace">
      {actor && <aside className="sidebar">
        <div className="nav-caption">WORKSPACE</div>
        <nav aria-label="Main navigation">{navLinks.map((link) => <a key={link.path} href={link.path}
          className={`nav-link${path === link.path ? " active" : ""}`}
          aria-current={path === link.path ? "page" : undefined}
          onClick={(event) => { event.preventDefault(); navigate(link.path); }}>{link.label}</a>)}</nav>
        <div className="sidebar-bottom"><span className="status-dot" /> Demo workspace</div>
      </aside>}
      <main className="main-content">{children}</main>
    </div>
  </div>;
}
