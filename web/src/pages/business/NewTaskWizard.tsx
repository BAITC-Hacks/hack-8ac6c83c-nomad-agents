import { useState } from "react";
import { apiFetch, ApiError } from "../../lib/http";
import { CardStep } from "../../components/wizard/CardStep";
import { ClarifyStep } from "../../components/wizard/ClarifyStep";
import { DraftStep } from "../../components/wizard/DraftStep";
import { PublishedStep } from "../../components/wizard/PublishedStep";
import { RatingStep } from "../../components/wizard/RatingStep";
import { Stepper, type WizardStep } from "../../components/wizard/Stepper";
import "../../components/wizard/wizard.css";

export const FIELD_KEYS = ["title", "context", "need", "users", "data", "constraints", "expectedResult", "successCriteria", "contact", "interactionFormat", "topics", "techTags"] as const;
export type CardFields = Record<Exclude<(typeof FIELD_KEYS)[number], "topics" | "techTags">, string> & { topics: string[]; techTags: string[] };
export const EMPTY_FIELDS: CardFields = { title: "", context: "", need: "", users: "", data: "", constraints: "", expectedResult: "", successCriteria: "", contact: "", interactionFormat: "", topics: [], techTags: [] };
export type Analysis = {
  title: string;
  missingFields: string[];
  questions: { id: string; fieldKey: string; question: string; chips: string[] }[];
  suggestions: { fieldKey: string; action: string }[];
  extracted: { fieldKey: string; value: string; evidence: string }[];
  source: "ai" | "stub";
};
export type Rating = {
  total: number;
  level: "draft" | "workable" | "ready" | "priority";
  breakdown: { criterion: string; weight: number; score: number; reason: string; matchedSignals: string[] }[];
  missingDetails: { criterion: string; detail: string }[];
  quests: { criterion: string; fieldKey: string; action: string; potentialPoints: number }[];
  source: "rules" | "seed" | "cache";
  ratingRulesVersion: string;
  scoredAt: string;
  nextLevel?: { level: "draft" | "workable" | "ready" | "priority"; pointsNeeded: number };
};
export type Task = { id: string; fields?: Partial<CardFields>; answersApplied?: boolean; analysis?: Analysis; rating?: Rating | null; status?: string };
const fieldLabels: Record<string, string> = { title: "Title", context: "Context", need: "Need", users: "Users", data: "Data & materials", constraints: "Constraints", expectedResult: "Expected result", successCriteria: "Success criteria", contact: "Contact", interactionFormat: "Interaction format", topics: "Topics", techTags: "Technologies" };

export function labelForField(key: string) { return fieldLabels[key] ?? key; }

export default function NewTaskWizard() {
  const [step, setStep] = useState<WizardStep>(1);
  const [task, setTask] = useState<Task | null>(null);
  const [analysis, setAnalysis] = useState<Analysis | null>(null);
  const [fields, setFields] = useState<CardFields>(EMPTY_FIELDS);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [appliedAnswers, setAppliedAnswers] = useState<{ questionId: string; fieldKey: string; text: string }[]>([]);
  const [rating, setRating] = useState<Rating | null>(null);
  const [previousRating, setPreviousRating] = useState<Rating | null>(null);
  const [delta, setDelta] = useState(0);
  const [position, setPosition] = useState<number | null>(null);
  const [catalogCount, setCatalogCount] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [draftInput, setDraftInput] = useState<{ rawDraft: string; industry: string } | null>(null);

  const showError = (reason: unknown) => {
    if (reason instanceof ApiError) {
      const fieldError = Object.values(reason.problem.errors ?? {}).flat()[0];
      setError(fieldError ? `${reason.message}: ${fieldError}` : reason.message);
      return;
    }
    setError(reason instanceof Error ? reason.message : "Something went wrong. Please try again.");
  };

  const analyze = async (rawDraft: string, industry: string) => {
    setDraftInput({ rawDraft, industry });
    setBusy(true); setError("");
    try {
      const created = task ?? await apiFetch<Task>("/api/tasks", { method: "POST", body: JSON.stringify({ rawDraft, industry }) });
      setTask(created);
      setFields({ ...EMPTY_FIELDS, ...(created.fields ?? {}) });
      const result = await apiFetch<Analysis>(`/api/tasks/${created.id}/analyze`, { method: "POST" });
      setAnalysis(result);
      setAnswers(Object.fromEntries(result.questions.map((q) => [q.id, ""])));
      setStep(2);
    } catch (reason) { showError(reason); }
    finally { setBusy(false); }
  };

  const applyAnswers = async () => {
    if (!task || !analysis) return;
    const payload = analysis.questions.map((q) => ({ questionId: q.id, fieldKey: q.fieldKey, text: answers[q.id]?.trim() ?? "" }));
    setBusy(true); setError("");
    try {
      const saved = await apiFetch<Task>(`/api/tasks/${task.id}/answers`, { method: "PUT", body: JSON.stringify({ answers: payload }) });
      setTask(saved); setAppliedAnswers(payload); setFields({ ...EMPTY_FIELDS, ...(saved.fields ?? {}) }); setStep(3);
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 409) setError("These answers were already applied with different text. Continue in the card editor to change the fields.");
      else showError(reason);
    } finally { setBusy(false); }
  };

  const saveFields = async (updated: CardFields) => {
    if (!task) return;
    setBusy(true); setError("");
    try {
      const saved = await apiFetch<Task>(`/api/tasks/${task.id}/fields`, { method: "PUT", body: JSON.stringify({ fields: updated }) });
      setTask(saved); setFields({ ...EMPTY_FIELDS, ...(saved.fields ?? updated) });
    } catch (reason) { showError(reason); throw reason; }
    finally { setBusy(false); }
  };

  const confirm = async (updated: CardFields) => {
    if (!task) return;
    setBusy(true); setError("");
    try {
      // Persist edits first so the confirmation always scores the exact form the user reviewed.
      const saved = await apiFetch<Task>(`/api/tasks/${task.id}/fields`, { method: "PUT", body: JSON.stringify({ fields: updated }) });
      const response = await apiFetch<{ task: Task; rating: Rating; delta: number; position?: number | { position: number; total: number } }>(`/api/tasks/${task.id}/confirm`, { method: "POST" });
      setPreviousRating(rating);
      setRating(response.rating); setTask(response.task ?? saved); setFields({ ...EMPTY_FIELDS, ...(response.task?.fields ?? updated) }); setDelta(response.delta);
      if (typeof response.position === "number") setPosition(response.position);
      else if (response.position) { setPosition(response.position.position); setCatalogCount(response.position.total); }
      setStep(4);
    } catch (reason) { showError(reason); }
    finally { setBusy(false); }
  };

  const publish = async () => {
    if (!task || !rating || !fields.title.trim()) return;
    setBusy(true); setError("");
    try {
      const published = await apiFetch<Task>(`/api/tasks/${task.id}/publish`, { method: "POST" });
      setTask(published); setStep(5);
    } catch (reason) { showError(reason); }
    finally { setBusy(false); }
  };

  const focusField = (key: string) => {
    setStep(3);
    window.setTimeout(() => document.getElementById(`field-${key}`)?.focus(), 0);
  };

  return <main className="wizard-shell">
    <header className="wizard-heading"><div><p className="wizard-eyebrow">BUSINESS WORKSPACE</p><h1>Create a task</h1><p>Shape a clear challenge for student teams.</p></div>{task && <span className="wizard-task-id">Draft · {task.id}</span>}</header>
    <Stepper step={step} onStep={setStep} canNavigate={Boolean(task)} />
    {error && <div className="wizard-error" role="alert">{error}</div>}
    {busy && <p className="wizard-busy" role="status">Saving your changes…</p>}
    {step === 1 && <DraftStep busy={busy} initialDraft={draftInput?.rawDraft} initialIndustry={draftInput?.industry} onAnalyze={analyze} />}
    {step === 2 && analysis && <ClarifyStep analysis={analysis} answers={answers} appliedAnswers={appliedAnswers} busy={busy} onAnswer={(id, value) => setAnswers((prev) => ({ ...prev, [id]: value }))} onApply={applyAnswers} onEditCard={() => setStep(3)} />}
    {step === 3 && task && <CardStep fields={fields} analysis={analysis} rating={rating} busy={busy} onChange={(key, value) => setFields((prev) => ({ ...prev, [key]: value }))} onSave={saveFields} onConfirm={confirm} onFocusField={focusField} />}
    {step === 4 && rating && <RatingStep current={rating} previous={previousRating} delta={delta} fields={fields} busy={busy} onImprove={() => setStep(3)} onPublish={publish} />}
    {step === 5 && task && <PublishedStep taskId={task.id} position={position} catalogCount={catalogCount} />}
  </main>;
}
