import { createContext, useContext, useState, type ReactNode } from "react";

export type ActorRole = "business" | "team";

export interface Actor {
  role: ActorRole;
  actorId: string;
}

let activeActor: Actor | null = null;

const ActorContext = createContext<{
  actor: Actor | null;
  setActor: (actor: Actor | null) => void;
} | null>(null);

export function getCurrentActor() { return activeActor; }

export function ActorProvider({ children }: { children: ReactNode }) {
  const [actor, updateActor] = useState<Actor | null>(activeActor);
  const setActor = (next: Actor | null) => {
    activeActor = next;
    updateActor(next);
  };

  return <ActorContext.Provider value={{ actor, setActor }}>{children}</ActorContext.Provider>;
}

export function useActor() {
  const ctx = useContext(ActorContext);
  if (!ctx) throw new Error("useActor must be used within an ActorProvider");
  return ctx;
}
