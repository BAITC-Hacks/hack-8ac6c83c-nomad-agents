export type ReadinessLevel = "draft" | "workable" | "ready" | "priority";

export interface TaskRating {
  total: number;
  level: ReadinessLevel;
  breakdown: { criterion: string; weight: number; score: number; reason: string; matchedSignals?: string[] }[];
  missingDetails: { criterion: string; detail: string }[];
  quests: { criterion: string; fieldKey: string; action: string; potentialPoints: number }[];
  source: "rules" | "seed" | "cache";
  ratingRulesVersion?: string;
  scoredAt?: string;
}

export interface TaskCardFields {
  title: string;
  context?: string;
  need?: string;
  users?: string;
  data?: string;
  constraints?: string;
  expectedResult?: string;
  successCriteria?: string;
  contact?: string;
  interactionFormat?: string;
  topics?: string[];
  techTags?: string[];
}

export interface CatalogTask {
  id: string;
  title: string;
  businessName: string;
  summary?: string;
  shortSummary?: string;
  status?: "editing" | "published";
  rating: TaskRating | null;
  position?: number;
  catalogPosition?: number;
  catalogTotal?: number;
  proposalCount: number;
  topics: string[];
  techTags: string[];
  fields?: TaskCardFields;
  confirmedFields?: TaskCardFields;
  confirmed?: { fields: TaskCardFields; confirmedAt?: string } | null;
  businessId?: string;
  hasUnconfirmedChanges?: boolean;
}

export interface ProposalSummary {
  id?: string;
  taskId: string;
  teamId: string;
  status: "pending" | "selected" | "rejected";
  idea: string;
  plan: string;
  timeline: string;
  prototypeUrl: string;
}

export interface CatalogResponse {
  items: CatalogTask[];
  total: number;
}

export function confirmedFields(task: CatalogTask): TaskCardFields {
  return task.confirmed?.fields ?? task.confirmedFields ?? task.fields ?? { title: task.title };
}

export function taskSummary(task: CatalogTask): string {
  const fields = confirmedFields(task);
  return task.shortSummary ?? task.summary ?? fields.need ?? fields.context ?? "Confirmed task details are available.";
}
