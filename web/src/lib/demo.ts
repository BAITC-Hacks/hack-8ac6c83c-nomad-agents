import type { Actor } from "./actor";

// Browser-only sample data. This is never sent to the API and is deliberately
// marked as a demo throughout the UI. It keeps the walkthrough usable while
// the backend endpoints are still being built.
type Fields = Record<string, string | string[]>;
type Task = { id: string; businessId: string; businessName: string; rawDraft: string; industry: string; status: string; fields: Fields; confirmed?: { fields: Fields; confirmedAt: string }; rating?: ReturnType<typeof sampleRating>; answersApplied?: boolean; answerHash?: string; analysis?: ReturnType<typeof analyze>; hasUnconfirmedChanges?: boolean };
type Proposal = { id: string; taskId: string; taskTitle: string; teamId: string; teamName: string; tags: string[]; idea: string; plan: string; timeline: string; prototypeUrl: string; status: "pending" | "selected" | "rejected"; reason?: string; milestones: { id: string; title: string }[] };
type DemoState = { tasks: Task[]; proposals: Proposal[]; logs: unknown[] };
const KEY = "taskforge.demo.v1";
const businesses = [
  { id: "b-nomad", name: "Nomad Logistics", industry: "Logistics", contactName: "Aigerim" },
  { id: "b-steppe", name: "Steppe Retail", industry: "Retail", contactName: "Dana" },
  { id: "b-tamaq", name: "Tamaq Café Chain", industry: "Food & Beverage", contactName: "Mira" },
];
const teams = [
  { id: "t-bytenomads", name: "Byte Nomads", tags: ["react", "python", "ml", "logistics", "food"] },
  { id: "t-nullptr", name: "Null Pointers", tags: ["dotnet", "openai", "react", "retail", "nlp"] },
];
const sampleFields: Fields = {
  title: "Delivery delay prediction dashboard", context: "Manual dispatching makes 18% of deliveries late.",
  need: "Help dispatchers spot likely delays before they happen.", users: "12 dispatchers review routes daily.",
  data: "Two years of CSV route logs, about 400k rows, plus a weather API.",
  constraints: "Six weeks; React and Python; read-only database replica.",
  expectedResult: "A web dashboard and delay prediction model.",
  successCriteria: "Predict delays of at least 30 minutes with 75% precision on a holdout month.",
  contact: "Aigerim, logistics lead", interactionFormat: "Weekly 30-minute call and Slack feedback.",
  topics: ["logistics", "data-analytics"], techTags: ["react", "python", "ml"],
};
const steppeFields: Fields = {
  title: "Customer review analysis", context: "Customer reviews arrive through several channels.",
  need: "Understand common complaints.", users: "Retail staff", data: "We have reviews.", constraints: "",
  expectedResult: "An analysis tool.", successCriteria: "", contact: "Dana", interactionFormat: "",
  topics: ["retail", "nlp"], techTags: ["dotnet", "openai"],
};
const criteria = [
  ["Problem clarity", "need", 20], ["Users", "users", 15], ["Data & materials", "data", 15],
  ["Constraints", "constraints", 10], ["Expected result", "expectedResult", 15],
  ["Success criteria", "successCriteria", 15], ["Collaboration", "interactionFormat", 10],
] as const;
function sampleRating(fields: Fields) {
  const breakdown = criteria.map(([criterion, fieldKey, weight]) => {
    const value = String(fields[fieldKey] ?? "").trim();
    const score = !value ? 0 : value.length < 20 ? Math.round(weight * .6) : value.length < 40 ? Math.round(weight * .75) : weight;
    return { criterion, weight, score, reason: value ? "Sample estimate based on supplied detail." : "Add this detail to the card.", matchedSignals: [] };
  });
  const total = breakdown.reduce((sum, item) => sum + item.score, 0);
  const level = total >= 90 ? "priority" : total >= 70 ? "ready" : total >= 40 ? "workable" : "draft";
  const next = [[40, "workable"], [70, "ready"], [90, "priority"]] as const;
  const threshold = next.find(([points]) => points > total);
  return { total, level, breakdown, missingDetails: breakdown.filter(item => item.score === 0).map(item => ({ criterion: item.criterion, detail: `Add ${item.criterion.toLowerCase()} details.` })),
    quests: breakdown.filter(item => item.score < item.weight).map(item => { const fieldKey = criteria.find(c => c[0] === item.criterion)![1]; return { criterion: item.criterion, fieldKey, action: `Add specific ${item.criterion.toLowerCase()} evidence`, potentialPoints: item.weight - item.score }; }),
    source: "seed", ratingRulesVersion: "sample-only", scoredAt: new Date().toISOString(), nextLevel: threshold ? { level: threshold[1], pointsNeeded: threshold[0] - total } : undefined };
}
function analyze(draft: string) {
  const keys = ["users", "data", "expectedResult", "successCriteria", "constraints"];
  const prompts: Record<string, string> = { users: "Who will use or benefit from the solution?", data: "What data or examples can the team access?", expectedResult: "What should the team deliver?", successCriteria: "How will you know it works?", constraints: "What time, technology, or access limits apply?" };
  return { title: "", missingFields: keys, source: "stub" as const,
    questions: keys.map((fieldKey, index) => ({ id: `q${index + 1}`, fieldKey, question: prompts[fieldKey], chips: fieldKey === "users" ? ["Customers", "Staff", "Managers"] : fieldKey === "expectedResult" ? ["Web app", "Dashboard", "Prototype"] : [] })),
    suggestions: keys.map(fieldKey => ({ fieldKey, action: prompts[fieldKey] })),
    extracted: [{ fieldKey: "context", value: draft, evidence: draft }],
  };
}
function seed(): DemoState {
  const now = new Date().toISOString();
  const nomad: Task = { id: "task-nomad", businessId: "b-nomad", businessName: "Nomad Logistics", rawDraft: "", industry: "Logistics", status: "published", fields: sampleFields, confirmed: { fields: sampleFields, confirmedAt: now }, rating: sampleRating(sampleFields) };
  const steppe: Task = { id: "task-steppe", businessId: "b-steppe", businessName: "Steppe Retail", rawDraft: "", industry: "Retail", status: "published", fields: steppeFields, confirmed: { fields: steppeFields, confirmedAt: now }, rating: sampleRating(steppeFields) };
  return { tasks: [nomad, steppe], proposals: [
    { id: "proposal-byte", taskId: nomad.id, taskTitle: String(sampleFields.title), teamId: "t-bytenomads", teamName: "Byte Nomads", tags: teams[0].tags, idea: "A prediction model with a dispatcher dashboard.", plan: "Prepare data, train a model, build dashboard, then validate with dispatchers.", timeline: "5 weeks", prototypeUrl: "", status: "pending", milestones: [] },
    { id: "proposal-null", taskId: nomad.id, taskTitle: String(sampleFields.title), teamId: "t-nullptr", teamName: "Null Pointers", tags: teams[1].tags, idea: "Rules-based alerts with explanations.", plan: "Map late-delivery patterns, implement alerts, then test the workflow.", timeline: "6 weeks", prototypeUrl: "", status: "pending", milestones: [] },
  ], logs: [] };
}
function read(): DemoState {
  try { const saved = localStorage.getItem(KEY); if (saved) return JSON.parse(saved) as DemoState; } catch { /* use seed */ }
  const state = seed(); write(state); return state;
}
function write(state: DemoState) { localStorage.setItem(KEY, JSON.stringify(state)); window.dispatchEvent(new Event("taskforge:demo-changed")); }
function fail(title: string): never { throw new Error(title); }
function body(raw: BodyInit | null | undefined): Record<string, any> { try { return JSON.parse(String(raw ?? "{}")); } catch { return {}; } }
function positions(state: DemoState) {
  return state.tasks.filter(t => t.status === "published").sort((a, b) => (b.rating?.total ?? 0) - (a.rating?.total ?? 0)).map((t, i) => ({ id: t.id, rank: i + 1, total: state.tasks.filter(x => x.status === "published").length }));
}
function item(t: Task, state: DemoState) {
  const pos = positions(state).find(p => p.id === t.id);
  const fields = t.confirmed?.fields ?? t.fields;
  return { ...t, title: String(fields.title ?? "Untitled task"), confirmedFields: fields, topics: fields.topics ?? [], techTags: fields.techTags ?? [], position: pos?.rank, catalogTotal: pos?.total, proposalCount: state.proposals.filter(p => p.taskId === t.id).length };
}
function owner(task: Task | undefined, actor: Actor | null) { if (!task || actor?.role !== "business" || actor.actorId !== task.businessId) fail("Task not found for this business."); return task; }
export function demoRequest<T>(url: string, method: string, raw: BodyInit | null | undefined, actor: Actor | null): T {
  const state = read(); const path = new URL(url, location.origin).pathname.replace(/^\/api/, ""); const input = body(raw);
  let result: unknown;
  if (path === "/actors" && method === "GET") result = { businesses, teams };
  else if (path === "/health") result = { status: "sample", aiMode: "stub", firestore: "unavailable" };
  else if (path === "/tasks" && method === "POST") {
    if (actor?.role !== "business") fail("Choose a business profile first.");
    const business = businesses.find(b => b.id === actor.actorId)!;
    const task: Task = { id: crypto.randomUUID(), businessId: business.id, businessName: business.name, rawDraft: String(input.rawDraft ?? ""), industry: String(input.industry ?? ""), status: "editing", fields: { title: "", context: "", need: "", users: "", data: "", constraints: "", expectedResult: "", successCriteria: "", contact: "", interactionFormat: "", topics: [], techTags: [] } };
    state.tasks.push(task); write(state); result = task;
  } else if (path === "/tasks/mine" && method === "GET") result = state.tasks.filter(t => t.businessId === actor?.actorId).map(t => item(t, state));
  else if (path === "/catalog/topics") result = [...new Set(state.tasks.filter(t => t.status === "published").flatMap(t => t.confirmed?.fields.topics ?? []).map(String))].sort();
  else if (path === "/catalog" && method === "GET") {
    const params = new URL(url, location.origin).searchParams; const topics = params.getAll("topic"); const levels = params.getAll("level");
    const all = positions(state).map(p => item(state.tasks.find(t => t.id === p.id)!, state));
    result = { items: all.filter(t => (!topics.length || topics.some(topic => (t.topics as string[]).includes(topic))) && (!levels.length || levels.includes(t.rating?.level ?? "draft"))), total: all.length };
  } else if (path === "/leaderboard") result = teams.map(t => ({ teamId: t.id, name: t.name, points: state.proposals.filter(p => p.teamId === t.id).reduce((sum, p) => sum + p.milestones.length * 10, 0) })).sort((a, b) => b.points - a.points);
  else if (path === "/ai-logs") { const filter = new URL(url, location.origin).searchParams.get("taskId"); result = filter ? (state.logs as { taskId?: string }[]).filter(log => log.taskId?.includes(filter)) : state.logs; }
  else if (/^\/teams\/[^/]+\/recommendations$/.test(path)) { const team = teams.find(t => t.id === path.split("/")[2]); result = { teamName: team?.name, items: positions(state).map(p => { const task = item(state.tasks.find(t => t.id === p.id)!, state); return { task, matchedTags: [...(task.topics as string[]), ...(task.techTags as string[])].filter(tag => team?.tags.includes(tag)) }; }).filter(x => x.matchedTags.length).slice(0, 3) }; }
  else if (path === "/proposals/mine") result = state.proposals.filter(p => p.teamId === actor?.actorId);
  else if (/^\/proposals\/[^/]+\/decision$/.test(path) && method === "POST") {
    const proposal = state.proposals.find(p => p.id === path.split("/")[2]); owner(state.tasks.find(t => t.id === proposal?.taskId), actor); if (!proposal) fail("Proposal not found.");
    if (proposal.milestones.length && input.decision === "pending") fail("A milestone has already been confirmed.");
    proposal.status = input.decision; proposal.reason = input.reason; write(state); result = proposal;
  } else if (/^\/proposals\/[^/]+\/milestones$/.test(path) && method === "POST") {
    const proposal = state.proposals.find(p => p.id === path.split("/")[2]); owner(state.tasks.find(t => t.id === proposal?.taskId), actor); if (!proposal || proposal.status !== "selected") fail("Select the proposal first.");
    if (!String(input.title ?? "").trim()) fail("Milestone title is required.");
    const id = String(input.id ?? crypto.randomUUID()); if (!proposal.milestones.some(m => m.id === id)) proposal.milestones.push({ id, title: String(input.title) });
    write(state); result = proposal;
  } else if (/^\/catalog\/[^/]+$/.test(path)) { const task = state.tasks.find(t => t.id === path.split("/")[2] && t.status === "published"); result = task ? item(task, state) : fail("Task not found."); }
  else if (/^\/tasks\/[^/]+/.test(path)) {
    const parts = path.split("/"); const task = state.tasks.find(t => t.id === parts[2]);
    if (parts[3] === "proposals") {
      if (parts[4] === "mine") {
        if (actor?.role !== "team") fail("Choose a team profile first.");
        let proposal = state.proposals.find(p => p.taskId === task?.id && p.teamId === actor.actorId);
        if (method === "PUT") {
          if (!task || task.status !== "published") fail("Task is not published.");
          if (proposal && proposal.status !== "pending") fail("Decided proposals cannot be edited.");
          const idea = String(input.idea ?? "").trim(), plan = String(input.plan ?? "").trim(), timeline = String(input.timeline ?? "").trim(), prototypeUrl = String(input.prototypeUrl ?? "").trim();
          let validUrl = true; try { if (prototypeUrl) validUrl = ["http:", "https:"].includes(new URL(prototypeUrl).protocol) && !!new URL(prototypeUrl).hostname; } catch { validUrl = false; }
          if (idea.length < 20 || idea.length > 2000 || plan.length < 20 || plan.length > 3000 || timeline.length < 3 || timeline.length > 200 || !validUrl) fail("Check proposal lengths and use a valid http(s) prototype URL.");
          if (!proposal) { proposal = { id: crypto.randomUUID(), taskId: task.id, taskTitle: String(task.confirmed?.fields.title ?? ""), teamId: actor.actorId, teamName: teams.find(t => t.id === actor.actorId)?.name ?? actor.actorId, tags: teams.find(t => t.id === actor.actorId)?.tags ?? [], status: "pending", idea, plan, timeline, prototypeUrl, milestones: [] }; state.proposals.push(proposal); }
          else Object.assign(proposal, { idea, plan, timeline, prototypeUrl });
          write(state);
        }
        result = proposal ?? null;
      } else { owner(task, actor); result = state.proposals.filter(p => p.taskId === task?.id); }
    } else {
      owner(task, actor);
      if (method === "GET") result = item(task!, state);
      else if (parts[3] === "analyze" && method === "POST") {
        task!.analysis = analyze(task!.rawDraft); state.logs.unshift({ id: crypto.randomUUID(), createdAt: new Date().toISOString(), kind: "analyze", taskId: task!.id, model: "local basic questions", validation: "fallback-stub", latencyMs: 0, systemPrompt: "Local basic question template. No external AI call was made.", input: { rawDraft: task!.rawDraft }, rawOutput: task!.analysis, errors: ["API unavailable"] }); write(state); result = task!.analysis;
      } else if (parts[3] === "answers" && method === "PUT") {
        const hash = JSON.stringify(input.answers ?? []); if (task!.answerHash && task!.answerHash !== hash) fail("Answers already applied. Edit the card instead.");
        if (!task!.answerHash) { for (const answer of input.answers ?? []) if (answer.text?.trim()) task!.fields[answer.fieldKey] = answer.text.trim(); task!.answerHash = hash; task!.answersApplied = true; write(state); } result = task;
      } else if (parts[3] === "fields" && method === "PUT") { task!.fields = { ...task!.fields, ...input.fields }; task!.hasUnconfirmedChanges = !!task!.confirmed; write(state); result = task; }
      else if (parts[3] === "confirm" && method === "POST") { const previous = task!.rating?.total ?? 0; task!.rating = sampleRating(task!.fields); task!.confirmed = { fields: { ...task!.fields }, confirmedAt: new Date().toISOString() }; task!.hasUnconfirmedChanges = false; write(state); result = { task: item(task!, state), rating: task!.rating, delta: task!.rating.total - previous, position: positions(state).find(p => p.id === task!.id) }; }
      else if (parts[3] === "publish" && method === "POST") { if (!task!.confirmed || !String(task!.confirmed.fields.title ?? "").trim() || task!.hasUnconfirmedChanges) fail("Confirm a titled card before publishing."); task!.status = "published"; write(state); result = item(task!, state); }
    }
  }
  if (result === undefined) fail(`Sample route unavailable: ${method} ${path}`);
  return structuredClone(result) as T;
}
