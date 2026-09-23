# Technical Specification
## AI Challenge Coach — Gamified Business Task Readiness Platform

**Document type:** MVP Technical Specification  
**Hackathon context:** AI Sana / HackAlem — Gamification of Practical Assignments  
**Target implementation time:** 5 hours  
**Primary goal:** Build a working end-to-end MVP that helps business representatives turn weak problem statements into implementation-ready challenge cards, scores their readiness, publishes them in a ranked catalog, and allows student teams to submit proposals.

---

## 1. Product Vision

The system gamifies the **quality of a business problem statement**, not the students themselves.

The core product idea is:

> **We do not gamify students. We gamify the quality of the business problem.**

A business representative starts with a short, incomplete request such as:

> “We need AI to process customer requests.”

The platform analyzes the description, identifies missing information, asks clarification questions, updates the task card, recalculates its readiness score, and visibly moves the task toward higher readiness levels.

The primary engagement loop is:

```text
Draft
  ↓
AI Analysis
  ↓
Clarification Questions
  ↓
Task Card Improvement
  ↓
Readiness Score Increase
  ↓
Higher Catalog Position
  ↓
More Attractive Task for Student Teams
```

---

## 2. MVP Scope

The MVP must support the following end-to-end scenario:

1. Business user enters a short task description.
2. AI analyzes the task and detects missing information.
3. AI generates at least three relevant clarification questions.
4. Business user answers the questions.
5. The system generates or updates an editable structured task card.
6. The system calculates a readiness score from 0 to 100.
7. The system shows what information is missing and how many points can be gained.
8. The business user confirms the task card.
9. The task is published in a public catalog.
10. Catalog position is influenced by readiness score.
11. A student team opens the task and submits a proposal.
12. The business user manually accepts or rejects the proposal.

---

## 3. User Roles

### 3.1 Business Representative

Capabilities:

- create a draft task;
- answer clarification questions;
- edit AI-generated task card fields;
- view readiness score and score breakdown;
- see improvement recommendations;
- publish a task;
- review student proposals;
- accept or reject proposals manually.

### 3.2 Student Team

Capabilities:

- browse the task catalog;
- filter tasks;
- view readiness score and task details;
- submit a solution proposal;
- include solution idea, implementation plan, estimated duration, and prototype link.

### 3.3 AI Challenge Coach

Logical AI role responsible for:

- identifying missing information;
- generating clarification questions;
- transforming answers into structured task data;
- proposing task-quality improvements;
- never inventing facts not provided by the user.

---

## 4. Core Gamification Model

### 4.1 Readiness Score

Each task has a score from `0` to `100`.

The score represents **how ready the task is for a student team to start working on it**.

It does not represent:

- company prestige;
- company size;
- popularity;
- number of proposals;
- student preference.

### 4.2 Score Breakdown

| Criterion | Maximum Score |
|---|---:|
| Context and Business Need | 20 |
| Data and Materials | 20 |
| Expected Result | 15 |
| Success Criteria | 15 |
| Constraints | 10 |
| Target Users | 10 |
| Business Communication | 10 |
| **Total** | **100** |

### 4.3 Readiness Levels

| Score | Level | Meaning |
|---:|---|---|
| 0–39 | Draft | Requires clarification |
| 40–69 | Working | Teams may respond; task may be recommended |
| 70–89 | Ready | Task receives higher catalog visibility |
| 90–100 | Priority | Task is fully prepared and visually highlighted |

### 4.4 Gamification Feedback

The UI should always answer three questions:

1. **Where am I now?**
2. **What information is missing?**
3. **What should I do next to improve the score?**

Example:

```text
Task Readiness

██████████████░░░░░░ 68/100

Level: WORKING

Next level: READY at 70

Suggested improvements:
+15 Add measurable success criteria
+10 Add constraints
+5  Improve the description of available data
```

---

## 5. Gamification Mechanics

### 5.1 Progress Bar

Every task editing screen must show a readiness progress indicator.

Example:

```text
68 / 100
WORKING
2 points to READY
```

### 5.2 Level-Up Events

When the score crosses a threshold, the UI should display a visible level-up event.

Example:

```text
68 → 83

Task upgraded!

WORKING → READY
```

Optional UI enhancement:

- lightweight confetti animation;
- badge change;
- animated progress bar.

### 5.3 Improvement Quests

Missing task information should be shown as actionable “quests”.

Example:

```text
Improve Your Challenge

Quest 1 — Show Your Data
Reward: +20 points
Describe available datasets, examples, files, or sources.

[Add Data]

Quest 2 — Define Success
Reward: +15 points
Explain how the business will determine whether the solution works.

[Add Success Criteria]

Quest 3 — Define Constraints
Reward: +10 points
Add deadline, technical, legal, access, or integration limitations.

[Add Constraints]
```

### 5.4 Catalog Position

Readiness score must influence task visibility.

Default sorting:

```text
score DESC
```

Example:

```text
#1  96  PRIORITY   AI Sales Assistant
#2  91  PRIORITY   Document Analyzer
#3  83  READY      Customer Support AI
#4  79  READY      Logistics Optimization
#5  55  WORKING    Document Processing
```

A low score must not hide a task.

### 5.5 Before / After Visualization

The demo should visibly show score growth.

Example:

```text
Initial description
      ↓
23 / 100
      ↓
AI questions
      ↓
43 / 100
      ↓
More answers
      ↓
68 / 100
      ↓
Success criteria added
      ↓
82 / 100
```

---

## 6. AI Functionality

At least one meaningful AI capability must be implemented.

Recommended design:

```text
Clarification Agent
        ↓
Task Structuring
        ↓
Scoring Agent
        ↓
Recommendation Agent
```

For the MVP, these may be implemented as separate logical prompts over the same LLM API.

---

## 7. Agent Responsibilities

### 7.1 Clarification Agent

Input:

```json
{
  "description": "We want to automate HR work with CVs.",
  "existingTask": {}
}
```

Responsibilities:

- analyze the description;
- determine which required fields are missing;
- generate at least three relevant questions;
- avoid asking for already known information;
- never invent data.

Expected output:

```json
{
  "missingFields": [
    "data",
    "successCriteria",
    "expectedResult"
  ],
  "questions": [
    {
      "field": "data",
      "question": "In what format are the CVs currently stored?"
    },
    {
      "field": "expectedResult",
      "question": "What should the solution produce for the HR team?"
    },
    {
      "field": "successCriteria",
      "question": "How will you measure whether the solution is successful?"
    }
  ]
}
```

---

### 7.2 Task Structuring Agent

Responsibilities:

- convert free-form description and answers into structured task fields;
- preserve only information explicitly provided by the user;
- generate editable wording;
- return structured JSON.

Expected output:

```json
{
  "title": "AI-Assisted CV Screening",
  "context": "The HR team manually reviews incoming CVs.",
  "need": "Reduce manual effort during initial candidate screening.",
  "users": "HR specialists",
  "data": "CV files in PDF format",
  "constraints": "",
  "expectedResult": "Rank or classify CVs according to predefined criteria.",
  "successCriteria": "",
  "businessContact": "",
  "interactionFormat": ""
}
```

---

### 7.3 Scoring Agent

Recommended implementation:

- deterministic scoring in backend code;
- optional LLM assistance only for semantic validation.

The score should not rely entirely on an LLM.

Example scoring result:

```json
{
  "totalScore": 45,
  "level": "WORKING",
  "breakdown": [
    {
      "field": "context",
      "score": 15,
      "maxScore": 20
    },
    {
      "field": "data",
      "score": 5,
      "maxScore": 20
    }
  ]
}
```

---

### 7.4 Recommendation Agent

Responsibilities:

- explain why points are missing;
- propose the next most valuable improvements;
- generate short, actionable recommendations.

Example:

```json
{
  "recommendations": [
    {
      "field": "successCriteria",
      "potentialPoints": 15,
      "message": "Add measurable criteria that define project success."
    },
    {
      "field": "constraints",
      "potentialPoints": 10,
      "message": "Specify deadline, technologies, access limitations, or integration constraints."
    }
  ]
}
```

---

## 8. Business Task Data Model

Recommended entity:

```csharp
public class BusinessTask
{
    public Guid Id { get; set; }

    public string Title { get; set; }
    public string RawDescription { get; set; }

    public string Context { get; set; }
    public string BusinessNeed { get; set; }
    public string TargetUsers { get; set; }

    public string DataAndMaterials { get; set; }
    public string Constraints { get; set; }

    public string ExpectedResult { get; set; }
    public string SuccessCriteria { get; set; }

    public string BusinessContact { get; set; }
    public string InteractionFormat { get; set; }

    public int ReadinessScore { get; set; }
    public ReadinessLevel ReadinessLevel { get; set; }

    public TaskStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## 9. Readiness Level Model

```csharp
public enum ReadinessLevel
{
    Draft,
    Working,
    Ready,
    Priority
}
```

Mapping:

```csharp
ReadinessLevel GetReadinessLevel(int score)
{
    return score switch
    {
        < 40 => ReadinessLevel.Draft,
        < 70 => ReadinessLevel.Working,
        < 90 => ReadinessLevel.Ready,
        _ => ReadinessLevel.Priority
    };
}
```

---

## 10. Proposal Data Model

```csharp
public class TeamProposal
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public string TeamName { get; set; }

    public string SolutionIdea { get; set; }
    public string ImplementationPlan { get; set; }

    public string EstimatedDuration { get; set; }

    public string PrototypeUrl { get; set; }

    public ProposalStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

```csharp
public enum ProposalStatus
{
    Pending,
    Accepted,
    Rejected
}
```

---

## 11. API Specification

### 11.1 Analyze Draft

```http
POST /api/tasks/analyze
```

Request:

```json
{
  "description": "We need AI to process customer requests."
}
```

Response:

```json
{
  "missingFields": [
    "data",
    "successCriteria",
    "constraints"
  ],
  "questions": [
    {
      "id": "q1",
      "field": "data",
      "text": "What historical customer request data is available?"
    },
    {
      "id": "q2",
      "field": "successCriteria",
      "text": "How will you measure whether the solution is successful?"
    },
    {
      "id": "q3",
      "field": "constraints",
      "text": "Are there technical, legal, or timing constraints?"
    }
  ]
}
```

---

### 11.2 Build or Update Task Card

```http
POST /api/tasks/build-card
```

Request:

```json
{
  "description": "We need AI to process customer requests.",
  "answers": [
    {
      "questionId": "q1",
      "answer": "We have 50,000 historical requests in CSV."
    },
    {
      "questionId": "q2",
      "answer": "Reduce average processing time by 30%."
    }
  ]
}
```

Response:

```json
{
  "task": {
    "title": "AI-Assisted Customer Request Processing",
    "context": "...",
    "businessNeed": "...",
    "dataAndMaterials": "50,000 historical requests in CSV",
    "successCriteria": "Reduce average processing time by 30%"
  }
}
```

---

### 11.3 Calculate Score

```http
POST /api/tasks/score
```

Request:

```json
{
  "taskId": "..."
}
```

Response:

```json
{
  "score": 82,
  "level": "READY",
  "breakdown": [],
  "recommendations": []
}
```

---

### 11.4 Publish Task

```http
POST /api/tasks/{taskId}/publish
```

Result:

```json
{
  "status": "published"
}
```

---

### 11.5 Get Catalog

```http
GET /api/tasks?sort=score_desc
```

Optional filters:

```text
topic
readinessLevel
status
```

---

### 11.6 Submit Proposal

```http
POST /api/tasks/{taskId}/proposals
```

Request:

```json
{
  "teamName": "Team Alpha",
  "solutionIdea": "Use classification and retrieval...",
  "implementationPlan": "1. Analyze data...",
  "estimatedDuration": "2 weeks",
  "prototypeUrl": "https://..."
}
```

---

### 11.7 Accept Proposal

```http
POST /api/proposals/{proposalId}/accept
```

---

### 11.8 Reject Proposal

```http
POST /api/proposals/{proposalId}/reject
```

---

## 12. Frontend Screens

### Screen 1 — Create Challenge

Components:

- large free-text input;
- primary CTA: `Analyze with AI`;
- short explanation of the process.

Example:

```text
Create Business Challenge

Describe your problem

┌───────────────────────────────────────┐
│ We want to automate customer          │
│ requests using AI...                  │
└───────────────────────────────────────┘

              [ Analyze with AI ]
```

---

### Screen 2 — AI Clarification

Components:

- AI Challenge Coach panel;
- at least three generated questions;
- user answers;
- continue button.

Example:

```text
AI Challenge Coach

I can help make your challenge ready for teams.

1. What data is available?
2. Who will use the solution?
3. How will you measure success?

[ Continue ]
```

---

### Screen 3 — Task Card Editor

Two-column layout recommended.

Left side:

- editable task fields.

Right side:

- readiness score;
- level;
- score breakdown;
- quests;
- next-level indicator.

Example:

```text
Task Readiness

68 / 100

WORKING

2 points to READY

Improve Your Challenge

+15 Define measurable success
+10 Add constraints
+5  Improve data description
```

---

### Screen 4 — Catalog

Each task card should show:

- title;
- short summary;
- readiness score;
- readiness level;
- topic;
- CTA to view details.

Sort by score descending by default.

---

### Screen 5 — Task Details

Show:

- full task card;
- readiness score;
- expected result;
- criteria;
- constraints;
- student CTA: `Submit Proposal`.

---

### Screen 6 — Submit Proposal

Fields:

- team name;
- solution idea;
- plan;
- estimated duration;
- prototype URL.

---

### Screen 7 — Business Proposal Review

Show all proposals for the task.

Actions:

```text
[ Accept ] [ Reject ]
```

No automatic team assignment.

---

## 13. Scoring Strategy

For hackathon reliability, use deterministic scoring.

Recommended rule:

- empty field: `0%` of field weight;
- partially filled field: `50%`;
- sufficiently detailed field: `100%`.

Example:

```csharp
int ScoreField(string value, int maxScore)
{
    if (string.IsNullOrWhiteSpace(value))
        return 0;

    if (value.Length < 30)
        return maxScore / 2;

    return maxScore;
}
```

A slightly better MVP version may combine:

```text
Deterministic completeness rules
+
LLM semantic quality classification
```

But the final score should remain explainable.

---

## 14. Recommended Technical Architecture

```text
┌────────────────────────────┐
│        Web Frontend        │
│ Angular / React            │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│        Backend API         │
│ ASP.NET Core Web API       │
└───────┬─────────┬──────────┘
        │         │
        │         ▼
        │   ┌──────────────────┐
        │   │ LLM / OpenAI API │
        │   └──────────────────┘
        │
        ▼
┌────────────────────────────┐
│ Persistence                │
│ In-memory / SQLite / JSON  │
└────────────────────────────┘
```

For a five-hour hackathon, prefer:

- ASP.NET Core;
- Angular or React;
- in-memory storage, SQLite, or local JSON;
- one LLM provider;
- no authentication unless time remains.

---

## 15. Suggested Internal Agent Architecture

```text
User Input
   │
   ▼
Clarification Agent
   │
   ▼
Structured Task Card
   │
   ▼
Scoring Engine
   │
   ├──► Readiness Score
   │
   ▼
Recommendation Agent
   │
   ▼
Improvement Quests
```

Important design decision:

The **Scoring Engine** should be backend logic, not purely an LLM prompt.

This makes the solution:

- deterministic;
- transparent;
- testable;
- easy to explain to judges.

---

## 16. AI Safety and Data Integrity Rules

The AI layer must follow these rules:

1. Never invent business facts.
2. Do not infer unavailable data.
3. Clearly distinguish user-provided facts from generated wording.
4. Every generated task card must be editable.
5. Business user must confirm the task before publication.
6. AI must not automatically select a student team.
7. AI may recommend tasks to students, but must not restrict access to the catalog.
8. Personal or sensitive participant characteristics must not be used for matching.

---

## 17. Error Handling

The MVP should gracefully handle:

### LLM unavailable

Fallback:

```text
AI service is temporarily unavailable.

You can continue editing the task manually.
```

Optional local fallback:

- static questions based on missing fields.

### Invalid LLM response

The backend should:

1. validate JSON;
2. reject malformed output;
3. retry once if time allows;
4. fall back to deterministic questions.

### Empty task description

Return validation error:

```json
{
  "error": "Task description is required."
}
```

---

## 18. Demo Scenario

Recommended prepared demo:

### Step 1 — Weak Task

Input:

```text
We want an AI solution for customer support.
```

Expected score:

```text
23 / 100
DRAFT
```

### Step 2 — AI Clarification

Questions:

```text
What customer support data is available?

Who will use the solution?

What should the system produce?

How will you measure success?
```

### Step 3 — User Answers

Example:

```text
We have 50,000 historical requests in CSV.

The users are support operators.

The system should classify requests and suggest answers.

Success means reducing average handling time by 30%.
```

### Step 4 — Score Increase

Display:

```text
23 → 82

Task upgraded!

DRAFT → READY
```

### Step 5 — Publish

The task appears in the catalog.

Example:

```text
Before: #11
After:  #3
```

### Step 6 — Student Proposal

Student team submits:

```text
Team Alpha

Idea:
Use intent classification and retrieval-augmented generation.

Plan:
1. Explore dataset
2. Build classifier
3. Add response suggestion
4. Build prototype
```

### Step 7 — Business Decision

Business clicks:

```text
[ Accept ]
```

This completes the required end-to-end scenario.

---

## 19. Hackathon Priorities

### Must Have

- draft input;
- AI-generated clarification questions;
- editable structured task card;
- deterministic readiness score;
- score explanation;
- task improvement recommendations;
- readiness levels;
- catalog;
- score-based sorting;
- student proposal;
- manual accept/reject;
- complete demo flow.

### Should Have

- animated score increase;
- level-up notification;
- quests;
- score breakdown;
- simple filtering.

### Nice to Have

- confetti;
- task recommendation;
- AI-generated task title;
- AI-generated concise summary;
- before/after visualization;
- task recommendation by team interests.

### Do Not Prioritize

- complex authentication;
- real-time chat;
- notification infrastructure;
- calendar integration;
- file storage;
- vector database;
- custom ML model;
- production deployment;
- complex team ranking;
- student XP;
- marketplace economy;
- badges unrelated to task quality.

---

## 20. Five-Hour Implementation Plan

### 0–30 min

- agree on data model;
- select stack;
- create repository;
- create basic UI routes;
- prepare synthetic data.

### 30–100 min

Implement:

- draft input;
- AI call;
- clarification questions;
- task card generation;
- task editor.

### 100–165 min

Implement:

- scoring engine;
- readiness levels;
- score breakdown;
- improvement recommendations;
- progress UI.

### 165–225 min

Implement:

- catalog;
- sorting;
- proposal form;
- proposal list;
- manual accept/reject.

### 225–270 min

Integrate:

- full end-to-end flow;
- validation;
- fallback logic;
- error handling.

### 270–300 min

Prepare:

- demo data;
- demo scenario;
- README;
- architecture diagram;
- backup screenshots;
- 5-minute presentation.

---

## 21. Definition of Done

The MVP is complete when judges can observe the following flow without slides:

```text
Weak business request
        ↓
AI clarification
        ↓
Structured task card
        ↓
Readiness score
        ↓
Task improvement
        ↓
Score increase
        ↓
Catalog publication
        ↓
Student proposal
        ↓
Business accepts or rejects proposal
```

All important transitions must work in the running application.

---

## 22. Key Product Message for the Demo

Recommended pitch:

> **AI Challenge Coach turns vague business needs into student-ready challenges.**

Supporting message:

> **Instead of gamifying students, we gamify the quality of the business problem.**

The key visible transformation is:

```text
"I need an AI solution."
          ↓
AI Challenge Coach
          ↓
82 / 100 — READY FOR DEVELOPMENT
```

This should be the central story of the product demo.

---

## 23. Future Extensions

Not required for the MVP:

- personalized task recommendations for students;
- semantic matching between team skills and tasks;
- task popularity analytics;
- comments and clarification threads;
- milestone tracking;
- team progress score;
- business feedback after project completion;
- team reputation based on completed work;
- company challenge history;
- notification system;
- calendar integration;
- portfolio generation for students.

---

## 24. Suggested Project Name Options

- AI Challenge Coach
- Challenge Level-Up
- TaskForge AI
- Ready2Build
- ChallengeRank
- ProblemReady AI
- Brief2Build
- ChallengeBoost

For the hackathon demo, **AI Challenge Coach** is the clearest option because it immediately communicates the role of the AI.
