# design.md — TaskForge Visual Design

Companion to `MVP_SPEC.md` (source of truth for behavior) and `docs/tasks/frontend/*` (source of truth for scope/sequencing). This document is the **visual/UX contract** for Dev A/Claude when building the frontend: what it should look like, which libraries to use, and how each P0 screen should be laid out. It does not change any API, data, or business rule from `MVP_SPEC.md`.

Current state: `web/` is Vite + React + TypeScript with hand-rolled inline styles and one global `styles.css` (see `AppShell.tsx`, `RatingPanel.tsx`, `LevelBadge.tsx`). No Tailwind or component library is installed yet, even though `MVP_SPEC.md` §10 decision #11 calls for Tailwind + shadcn/ui. This doc defines the target system; migration can happen incrementally, screen by screen, without breaking the working demo.

---

## 1. Design principles

1. **Laconic over decorative.** Every screen answers the Coach's three questions — *Where am I now? What's missing? What's next?* — with the fewest visual elements possible. No filler illustrations, no marketing copy.
2. **Score is the hero.** The readiness score/level is the single most prominent number in the business flow. Everything else (breakdown, quests) supports it.
3. **Calm, warm palette.** Beige/cream pastels instead of the current cool blue/violet SaaS look — approachable, low-glare, works for a long demo on a projector.
4. **Progressive disclosure.** Wizard steps, compact vs. full `RatingPanel`, collapsed AI log rows — never show more than the current step needs.
5. **Reuse, don't re-invent.** Use shadcn/ui primitives (built on Radix) for anything interactive (dialogs, tabs, tooltips, progress, accordion) instead of hand-rolled components. Keep custom components only for domain-specific pieces (RatingPanel, LevelBadge, ChipAnswer, Stepper).

---

## 2. Stack additions

Add to `web/`, without touching the API contract or `src/api` (generated client):

```bash
npm install -D tailwindcss postcss autoprefixer tailwindcss-animate
npx tailwindcss init -p
npm install class-variance-authority clsx tailwind-merge lucide-react
npm install @radix-ui/react-slot @radix-ui/react-progress @radix-ui/react-tabs \
            @radix-ui/react-dialog @radix-ui/react-tooltip @radix-ui/react-select \
            @radix-ui/react-checkbox @radix-ui/react-toast
```

- **Tailwind** — utility layer, replaces `styles.css` and `wizard.css` incrementally.
- **shadcn/ui pattern** — copy-in components under `web/src/components/ui/` (Button, Card, Badge, Progress, Tabs, Dialog, Select, Checkbox, Tooltip, Toast, Input, Textarea, Table). These are the "already existing visual libs" the brief asks to reuse: standard shadcn recipes, not custom-built.
- **lucide-react** — icon set (sparkles for AI, check-circle for confirm, trophy for leaderboard, arrow-right for quests). Keeps icon usage consistent and avoids emoji.
- **cva + clsx + tailwind-merge** — the standard shadcn `cn()` utility and variant pattern for Button/Badge.
- No animation library beyond `tailwindcss-animate` (used by shadcn for dialog/tooltip enter/exit) and CSS transitions already used in `RatingPanel` (progress bar width, score count-up). Do not add Framer Motion — it's unnecessary weight for a 4-hour build.

`tailwind.config.ts` content globs: `./index.html`, `./src/**/*.{ts,tsx}`.

---

## 3. Color system — beige pastel

Defined as CSS variables in `src/styles.css` (`:root`), consumed via Tailwind theme extension so components use `bg-background`, `text-foreground`, etc. rather than literal hex values.

| Token | Hex | Use |
|---|---|---|
| `--background` | `#FAF6EE` | App background (warm off-white beige) |
| `--surface` | `#FFFFFF` | Cards, panels, topbar |
| `--surface-muted` | `#F3ECDD` | Sidebar, secondary panels, table stripes |
| `--border` | `#E7DCC5` | Card borders, dividers |
| `--foreground` | `#4A4132` | Primary text (warm dark brown, not pure black) |
| `--muted-foreground` | `#8A7F68` | Secondary text, hints, captions |
| `--primary` | `#B08968` | Primary buttons, active nav, focus ring (warm caramel) |
| `--primary-foreground` | `#FFFFFF` | Text on primary |
| `--accent` | `#D8C3A5` | Hover states, subtle highlights |
| `--ring` | `#C9A876` | Focus ring |

Semantic/status colors stay desaturated pastels so they sit inside the beige family instead of clashing:

| Token | Hex | Use |
|---|---|---|
| `--level-draft` | text `#8A7F68` / bg `#EFE7D8` | "Needs clarification" badge |
| `--level-workable` | text `#7A8F6E` / bg `#E4EBD9` | Workable badge (sage) |
| `--level-ready` | text `#5E8C6A` / bg `#DCEBDF` | Ready badge (soft green) |
| `--level-priority` | text `#A9791F` / bg `#F7E7BE` | Priority badge (gold), + gold card border |
| `--success` | text `#5E8C6A` / bg `#E7F0E1` | Positive delta, toasts |
| `--danger` | text `#B4574A` / bg `#F6E3DE` | Negative delta, validation errors |
| `--info` | text `#7C6A9C` / bg `#EAE3F2` | AI-mode / stub badges |

Rules:
- Never use saturated blue/violet/red — this is the palette the current build uses (`#2563eb`, `#5c59e8`, `#a56c18`) and must be replaced.
- Priority level is the only card that gets a colored border (`--level-priority` gold, 2px) + a filled ★, per §4.3/§8.2 of the spec — everything else stays neutral beige/white so priority actually stands out.
- Dark mode is out of scope for the MVP (matches "no automated tests / hackathon scope" cut list); ship light-only.

---

## 4. Typography & spacing

- Font: keep **Inter** (already loaded), it's neutral and reads well at small sizes for tables/badges.
- Scale (Tailwind defaults, no custom scale needed): `text-xs` (12px) captions/badges, `text-sm` (14px) body/table, `text-base` (16px) form inputs, `text-2xl`/`text-3xl` for the score number and page H1.
- Weight: 700–800 for score numbers, level names, nav active state; 500–600 for labels/buttons; 400 for body copy.
- Spacing: 4px base unit (Tailwind default). Cards use `p-6`, page containers `max-w-5xl mx-auto`, section gaps `space-y-6`.
- Radius: soft, consistent — `rounded-xl` (12px) for cards, `rounded-full` for badges/pills, `rounded-lg` (8px) for buttons/inputs. No sharp corners anywhere (keeps the "pastel, friendly" read).
- Shadows: one soft shadow token (`shadow-sm`, e.g. `0 8px 24px -12px rgba(74,65,50,0.15)`) for cards; no heavy drop shadows.

---

## 5. Core components (map to `web/src/components/`)

All rebuilt on Tailwind + shadcn primitives; props/behavior unchanged from what F0–F6 briefs already specify.

- **AppShell.tsx** — topbar (brand mark in `--primary` circle, product label, RoleSwitcher, "AI logs" link) + sidebar nav (`--surface-muted` background, active link = `--primary` text on `--accent` pill) + `max-w-5xl` content area. Rebuild `.topbar`/`.sidebar`/`.nav-link` CSS classes as Tailwind utility classes on the same JSX structure.
- **RoleSwitcher.tsx** — shadcn `Select` (Radix), grouped optgroups Businesses/Teams, unchanged data contract.
- **LevelBadge.tsx** — shadcn `Badge` variant per level using the level color tokens in §3; same `aria-label`.
- **RatingPanel.tsx** — biggest score number in `text-4xl font-extrabold text-foreground`; shadcn `Progress` for the score bar with the 40/70/90 threshold ticks drawn as absolutely-positioned `div`s (same technique as today, just Tailwind classes); delta chip uses `--success`/`--danger` tokens with lucide `TrendingUp`/`TrendingDown` icons instead of ▲/▼ glyphs; breakdown as a simple `Table` (shadcn) with a mini progress bar per row; quests as a list of cards with an outline `Button` ("Add details") that scrolls/focuses the field, per spec.
- **ChipAnswer** (new, F1) — shadcn `Badge`-as-button (`variant="outline"`, `aria-pressed`) for each of the ≤4 chips; selecting one inserts its text into an adjacent shadcn `Textarea`, which stays editable. Empty chip list renders nothing (never a placeholder chip).
- **Stepper.tsx** (wizard) — shadcn `Tabs`-style horizontal stepper, non-interactive beyond the current/completed step (no skipping ahead), numbered circles filled `--primary` when done/current, `--border` when upcoming.
- **Toaster.tsx** — shadcn `Toast` (Radix), same ProblemDetails-driven content (`title` + first field error), danger styling from `--danger` tokens.
- **CatalogCard** (new, F3) — shadcn `Card`; priority = gold border + ★ top-right; draft = grey "Needs clarification" ribbon; score shown as a compact horizontal bar + number, not the full RatingPanel.
- **ProposalForm.tsx / ProposalReview.tsx** — shadcn `Card` + `Input`/`Textarea` + `Button`; review table uses shadcn `Table` with `Select`/`Button` group for Select/Reject actions and a `Dialog` confirm only if the decision reverses a prior one (spec §6.5 reversibility).
- **AiLogs.tsx** — shadcn `Accordion` per log row (collapsed by default: time/kind/task/model/validation/latency; expand → system prompt, input JSON, raw output in a `<pre>` block with `bg-surface-muted`).

---

## 6. Screen-by-screen layout notes (P0)

- **Business — New Task Wizard**: single-column Step 1 (draft textarea + industry select, generous whitespace, "Analyze with AI" primary button with lucide `Sparkles` icon). Step 2 becomes a **two-column** layout at `md:` breakpoint: question cards left, running list of "Suggestions" as a slim sidebar right. Step 3 is two columns: form left (scroll), Coach `RatingPanel` right, **sticky** (`sticky top-6`) so score stays visible while editing long fields. Step 4/5 collapse back to single column, centered, celebratory but restrained (a subtle beige confetti burst via CSS keyframes is enough — no external confetti library needed for "confetti-lite").
- **Catalog (team + business)**: filter bar (topic multi-select + level checkboxes as shadcn `Checkbox` group) pinned above a responsive card grid (`grid-cols-1 md:grid-cols-2 xl:grid-cols-3`), "Recommended for {team}" as a horizontally scrollable strip above the grid, visually distinct with `--accent` background band.
- **Task detail / Task view**: two-column on desktop (card content left ~60%, RatingPanel/Proposal panel right ~40%), stacking to one column below `md`.
- **My Tasks / My Proposals / Leaderboard**: single shadcn `Table`, sortable header only where the spec already requires an order (catalog sort is server-driven; these lists don't need client sort controls to stay laconic).

All P0 screens must render correctly at a 1280×800 laptop viewport for the demo; graceful mobile stacking is a nice-to-have, not a requirement (mobile layout is explicitly out of scope per §2).

---

## 7. Interaction & motion

- Buttons/links: 150ms color/background transition (Tailwind `transition-colors`).
- Score bar and delta: animate width/opacity over ~400ms on mount/update (already present in `RatingPanel`, keep it, just via Tailwind `transition-all duration-300`).
- Level-up notice: fade+slide in (`animate-in fade-in slide-in-from-top-1` from `tailwindcss-animate`), auto-persists (not a toast) since it's a key demo beat.
- Toasts: shadcn default slide-in from bottom-right, 5s auto-dismiss, manual close button.
- Loading states: text-based, no spinners with brand mismatch — "Analyzing your description…" as plain muted text with a lucide `Loader2` icon (`animate-spin`), consistent everywhere an async call is pending (analyze, confirm, publish, proposal decisions).
- Disabled states (e.g., "Apply answers" after one-time apply): `opacity-50 cursor-not-allowed` + tooltip explaining why (shadcn `Tooltip`), never just silently disabled.

---

## 8. Non-goals

- No redesign of data shape, routes, or API contract — this is styling/composition only.
- No new dependencies beyond §2 (no motion library, no charting library beyond the plain SVG/())div bars already used for the score and mini-bars).
- No dark mode, no i18n, no mobile-first layout (matches spec's explicit cuts).
- Do not restyle `web/src/api/**` (generated) or touch anything under `orval.config.ts`.
