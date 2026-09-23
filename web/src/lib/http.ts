import type { Actor } from "./actor";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";
const STORAGE_KEY = "taskforge.actor";

function currentActor(): Actor | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as Actor) : null;
  } catch {
    return null;
  }
}

export interface ProblemDetails {
  title?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(public problem: ProblemDetails, public status: number) {
    super(problem.title ?? `Request failed with status ${status}`);
  }
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const actor = currentActor();
  const headers = new Headers(init.headers);
  headers.set("Content-Type", "application/json");
  if (actor) {
    headers.set("X-Actor-Role", actor.role);
    headers.set("X-Actor-Id", actor.actorId);
  }

  const res = await fetch(`${BASE_URL}${path}`, { ...init, headers });

  if (!res.ok) {
    let problem: ProblemDetails = {};
    try {
      problem = await res.json();
    } catch {
      // no body
    }
    throw new ApiError(problem, res.status);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
