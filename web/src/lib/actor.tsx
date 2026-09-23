import { createContext, useContext, useEffect, useState, type ReactNode } from "react";

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
    return raw ? (JSON.parse(raw) as Actor) : null;
  } catch {
    return null;
  }
}

export function ActorProvider({ children }: { children: ReactNode }) {
  const [actor, setActor] = useState<Actor | null>(() => loadActor());

  useEffect(() => {
    if (actor) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(actor));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, [actor]);

  return <ActorContext.Provider value={{ actor, setActor }}>{children}</ActorContext.Provider>;
}

export function useActor() {
  const ctx = useContext(ActorContext);
  if (!ctx) throw new Error("useActor must be used within an ActorProvider");
  return ctx;
}
