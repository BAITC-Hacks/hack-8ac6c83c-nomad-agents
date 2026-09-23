import { getCurrentActor } from "./actor";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";

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
  const actor = getCurrentActor();
  const headers = new Headers(init.headers);
  if (init.body != null && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");
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
    notifyProblem(problem, res.status);
    throw new ApiError(problem, res.status);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
