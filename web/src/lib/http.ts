import type { Actor } from "./actor";
import { demoRequest } from "./demo";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";
const STORAGE_KEY = "taskforge.actor";
export const DEMO_EVENT = "taskforge:demo-mode";
let demoMode = false;
export function isDemoMode() { return demoMode; }
function enableDemo() {
  if (!demoMode) {
    demoMode = true;
    window.dispatchEvent(new Event(DEMO_EVENT));
  }
}

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

export interface ApiToast {
  title: string;
  detail?: string;
}

export const API_TOAST_EVENT = "taskforge:api-toast";

function notifyProblem(problem: ProblemDetails, status: number) {
  const firstError = Object.values(problem.errors ?? {}).flat().find(Boolean);
  const toast: ApiToast = {
    title: problem.title || `Request failed (${status})`,
    ...(firstError ? { detail: firstError } : {}),
  };
  window.dispatchEvent(new CustomEvent<ApiToast>(API_TOAST_EVENT, { detail: toast }));
}

export class ApiError extends Error {
  constructor(public problem: ProblemDetails, public status: number) {
    super(problem.title ?? `Request failed with status ${status}`);
  }
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const actor = currentActor();
  const headers = new Headers(init.headers);
  if (init.body != null && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");
  if (actor) {
    headers.set("X-Actor-Role", actor.role);
    headers.set("X-Actor-Id", actor.actorId);
  }

  if (demoMode) return demoRequest<T>(path, init.method ?? "GET", init.body, actor);
  let res: Response;
  try {
    res = await fetch(`${BASE_URL}${path}`, { ...init, headers });
  } catch {
    enableDemo();
    return demoRequest<T>(path, init.method ?? "GET", init.body, actor);
  }

  // The scaffold API only exposes health. A missing actors route means its
  // application contract is unavailable, so use one coherent local dataset.
  if (path === "/api/actors" && (res.status === 404 || res.status === 501)) {
    enableDemo();
    return demoRequest<T>(path, init.method ?? "GET", init.body, actor);
  }

  if (!res.ok) {
    let problem: ProblemDetails = {};
    try {
      problem = await res.json();
    } catch {
      // no body
    }
    notifyProblem(problem, res.status);
    throw new ApiError(problem, res.status);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
