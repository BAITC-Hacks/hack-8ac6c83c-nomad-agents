# Frontend task queue — MVP priorities

Source of truth: [MVP_SPEC.md](../../../MVP_SPEC.md), especially §§2, 8, 14–15. **P0 is the top priority and the demo gate.** Complete its full journey before P1 or P2.

| Priority | Brief | P0 result |
|---|---|---|
| P0 | [F0](F0-shell-and-role-switcher.md) | Role switcher, actor headers, business/team navigation |
| P0 | [F1](F1-wizard.md) | Draft → Analyze → questions and chips → editable card → Confirm twice → level-up → Publish |
| P0 | [F2](F2-rating-panel-coach.md) | Deterministic score display, breakdown, progress, quests, confirmed owner detail |
| P0 | [F3](F3-catalog-and-recommendations.md) | Shared catalog, topic + level filters, unfiltered position, team task view |
| P0 | [F4](F4-proposals-and-business-review.md) | Proposal submit/edit, business compare and manual Select / Reject |
| P1 | [F6](F6-ai-logs.md) | AI log viewer; P0 still documents prompt/schema and fallback in README |
| P2 | [F5](F5-leaderboard.md) | Leaderboard after milestone points are implemented |

P1 portions inside the P0 briefs are recommendations (F3), post-publish editing, preview and history (F2), and decision Reset (F4). P2 portions are milestone confirmation and points (F4), history sparkline (F2), and leaderboard (F5). Hide unfinished optional navigation and routes; do not present placeholder success data.

P0 acceptance path: switch to Tamaq → create weak draft → analyze → answer with chips/free text → apply once → edit card → confirm → follow a quest and confirm again → inspect score, breakdown, actual delta, and level-up if a threshold crossed → publish → filter catalog by topic and level → switch to a team and submit a proposal → switch to the owning business and Select / Reject manually. Catalog positions remain `#n of m` from the unfiltered published list. The backend's `full` seed profile and project README remain P0 submission deliverables even if this UI journey uses `demo`.
