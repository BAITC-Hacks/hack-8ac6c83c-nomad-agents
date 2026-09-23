export function PublishedStep({ taskId, position, catalogCount }: { taskId: string; position: number | null; catalogCount: number | null }) {
  return <section className="wizard-panel published-step"><div className="published-icon">✓</div><div className="panel-kicker">STEP 5 · PUBLISHED</div><h2>Your task is live</h2><p>Student teams can now discover your challenge and propose an approach.</p>
    {position !== null && <div className="catalog-position"><span>CATALOG POSITION</span><strong>#{position}{catalogCount ? <small> of {catalogCount}</small> : null}</strong></div>}
    <a className="primary-button link-button" href={`/business/tasks/${taskId}`}>View task details →</a><a className="text-link" href="/business/tasks">Back to My Tasks</a>
  </section>;
}
