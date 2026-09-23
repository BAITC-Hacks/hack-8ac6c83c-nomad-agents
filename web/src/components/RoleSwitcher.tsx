import { useActor } from "../lib/actor";
import type { ActorsDto } from "../App";

export function RoleSwitcher({ actors, error }: { actors: ActorsDto | null; error: string }) {
  const { actor, setActor } = useActor();
  const value = actor ? `${actor.role}:${actor.actorId}` : "";
  return <div className="actor-control">
    <label htmlFor="actor-select">Acting as</label>
    <select id="actor-select" value={value} onChange={(event) => {
      const [role, actorId] = event.target.value.split(":");
      if ((role === "business" || role === "team") && actorId) setActor({ role, actorId });
      else setActor(null);
    }}>
      <option value="">Choose a profile…</option>
      <optgroup label="Businesses">
        {actors?.businesses.map((business) => <option key={business.id} value={`business:${business.id}`}>{business.name}</option>)}
      </optgroup>
      <optgroup label="Teams">
        {actors?.teams.map((team) => <option key={team.id} value={`team:${team.id}`}>{team.name}</option>)}
      </optgroup>
    </select>
    {error && <span className="actor-error" title={error}>Profiles unavailable</span>}
  </div>;
}
