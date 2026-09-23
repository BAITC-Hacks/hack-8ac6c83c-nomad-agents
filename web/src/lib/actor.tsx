import { createContext, useContext, useState, type ReactNode } from "react";

export type ActorRole = "business" | "team";

export interface Actor {
  role: ActorRole;
  actorId: string;
}

const STORAGE_KEY = "taskforge.actor";

const ActorContext = createContext<{
  actor: Actor | null;
  setActor: (actor: Actor | null) => void;
} | null>(null);

function loadActor(): Actor | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const value: unknown = JSON.parse(raw);
    if (typeof value !== "object" || value === null) return null;
    const candidate = value as Partial<Actor>;
    return (candidate.role === "business" || candidate.role === "team") && typeof candidate.actorId === "string" && candidate.actorId
      ? { role: candidate.role, actorId: candidate.actorId }
      : null;
  } catch {
    return null;
  }
}

export function ActorProvider({ children }: { children: ReactNode }) {
  const [actor, updateActor] = useState<Actor | null>(() => loadActor());
  const setActor = (next: Actor | null) => {
    if (next) localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    else localStorage.removeItem(STORAGE_KEY);
    updateActor(next);
  };

  return <ActorContext.Provider value={{ actor, setActor }}>{children}</ActorContext.Provider>;
}

export function useActor() {
  const ctx = useContext(ActorContext);
  if (!ctx) throw new Error("useActor must be used within an ActorProvider");
  return ctx;
}
