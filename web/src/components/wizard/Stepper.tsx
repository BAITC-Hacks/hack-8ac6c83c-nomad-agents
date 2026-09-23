export type WizardStep = 1 | 2 | 3 | 4 | 5;
const steps = ["Draft", "Clarify", "Card", "Rating", "Publish"];
export function Stepper({ step, onStep, canNavigate }: { step: WizardStep; onStep: (step: WizardStep) => void; canNavigate: boolean }) {
  return <nav aria-label="Task creation progress" className="wizard-stepper">{steps.map((label, index) => {
    const number = (index + 1) as WizardStep;
    const current = step === number;
    const done = step > number;
    return <button type="button" key={label} className={`step-item${current ? " current" : ""}${done ? " done" : ""}`} aria-current={current ? "step" : undefined} disabled={number > step || (!canNavigate && number !== 1)} onClick={() => onStep(number)}>
      <span className="step-number">{done ? "✓" : number}</span><span>{label}</span>
    </button>;
  })}</nav>;
}
