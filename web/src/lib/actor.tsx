import { createContext, useCallback, useContext, useState, type ReactNode } from "react";

export type ActorRole = "business" | "team";

export interface Actor {
  role: ActorRole;
  actorId: string;
}

const STORAGE_KEY = "taskforge.actor";
let activeActor: Actor | null = null;

const ActorContext = createContext<{
  actor: Actor | null;
  setActor: (actor: Actor | null) => void;
} | null>(null);

export function getCurrentActor() { return activeActor; }

export function ActorProvider({ children }: { children: ReactNode }) {
  // Restore only after the live actor list has loaded and validated this choice.
  const [actor, updateActor] = useState<Actor | null>(null);
  const setActor = useCallback((next: Actor | null) => {
    activeActor = next;
    if (next) localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    else localStorage.removeItem(STORAGE_KEY);
    updateActor(next);
  }, []);

  return <ActorContext.Provider value={{ actor, setActor }}>{children}</ActorContext.Provider>;
}

export function restoreActor(actors: { businesses: { id: string }[]; teams: { id: string }[] }): Actor | null {
  let stored: unknown;
  try {
    stored = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? "null");
  } catch {
    localStorage.removeItem(STORAGE_KEY);
  }
  if (!stored || typeof stored !== "object") return null;
  const candidate = stored as Partial<Actor>;
  if ((candidate.role !== "business" && candidate.role !== "team") || typeof candidate.actorId !== "string") return null;
  const found = candidate.role === "business"
    ? actors.businesses.some(item => item.id === candidate.actorId)
    : actors.teams.some(item => item.id === candidate.actorId);
  if (!found) {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
  activeActor = { role: candidate.role, actorId: candidate.actorId };
  return activeActor;
}

export function useActor() {
  const ctx = useContext(ActorContext);
  if (!ctx) throw new Error("useActor must be used within an ActorProvider");
  return ctx;
}
