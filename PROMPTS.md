# AI-Assisted Development Notes

The assignment explicitly permits AI-assisted development and asks the
candidate to be ready to explain: how the requirement became a spec, what
prompts were used, what the AI generated, what was changed manually, what was
reviewed carefully, what assumptions were made, and what trade-offs were
chosen. This document is that write-up, kept honest about what actually
happened in this session.

## How the requirement became a specification

The starting point was the assignment's own `Problem_Statement -
Technical_Assignment.docx`, converted to plain text and read in full before
any code was written. Nothing in the spec was reinterpreted loosely — the
functional requirements, the "Must-Have" list, the suggested API scope, the
undo-per-mode examples, and the testing checklist were all extracted directly
from that document and used as the acceptance criteria for the code below.
Two places in the spec explicitly ask the implementer to choose and document
an approach rather than dictating one; those choices and the reasoning behind
them are written up in the README (§9 "Design Decisions"), not hidden.

## What was asked of the AI

The prompt driving this whole implementation was, in substance:

> "Build the project described in the attached problem statement. Explain
> every step, follow SOLID principles and OOP concepts."

That is a request for a **complete, working solution**, not a snippet or a
partial scaffold — so the AI was used to produce the full backend, frontend,
tests, and documentation in one pass, with an explicit steer toward clean
layering and principle-driven design rather than the shortest path to
"something that runs".

## What the AI generated

Effectively the entire repository:

- The full **`TicTacToe.Domain`**, **`TicTacToe.Application`**, and
  **`TicTacToe.Api`** projects (entities, rules, the computer strategy,
  DTOs, services, controllers, middleware, DI wiring).
- The full **Angular frontend** (models, `GameService`, and four feature
  components plus the root `AppComponent`).
- Backend unit tests (`TicTacToe.Domain.Tests`, `TicTacToe.Application.Tests`)
  and frontend unit tests (`GameService`, `GameBoardComponent`, `AppComponent`).
- This documentation (`README.md`, `PROMPTS.md`) and the project's `.gitignore`.

Every non-trivial class carries comments that name the specific SOLID/OOP
principle it demonstrates and *why* — e.g. why `Game` takes an `IWinChecker`
as a parameter instead of constructing one itself, why `Undo` rebuilds the
board from the surviving move history instead of trying to reverse a win
calculation, and why the frontend's feature components never call
`HttpClient` directly. That reasoning is left in the code deliberately, so it
can be pointed to directly during panel review instead of reconstructed from
memory.

## What a candidate should personally verify before submitting

This code was authored carefully and cross-checked by hand (including, for
several non-obvious cases — like the computer strategy's final fallback
branch — working out by hand *why* certain board states can or cannot occur,
which is documented next to the code it concerns). However, the AI could not
run `dotnet build`, `dotnet test`, `npm install`, or `ng serve` in the
environment it was generated in (no .NET SDK / no registry access for NuGet
were available there). **Before submitting, a candidate using this as a
starting point should personally**:

1. Run `dotnet restore && dotnet build` and `dotnet test` from `backend/`,
   and fix anything a real compiler catches that manual review didn't.
2. Run `npm install && npm start` from `frontend/`, click through every
   feature against the running backend, and run `npm test`.
3. Read through `Game.cs`, `GameService.cs`, and `BasicComputerStrategy.cs`
   in particular — these three carry the bulk of the actual game logic — and
   be able to explain each method's reasoning unprompted, since that is
   exactly what the panel review will ask for.
4. Decide whether the documented assumptions (README §10) match their own
   judgment, and adjust the code/README together if not.

## Trade-offs chosen (and why)

- **Undo disabled after game completion** (Option A) over reversing the
  scoreboard (Option B) — fewer states to reason about and test, appropriate
  for a review exercise; the trade-off is that a player who wants to "take
  back" the very last move of a finished game must use Reset Game and replay,
  rather than a literal undo. See README §9 for the full reasoning.
- **Hand-written test fakes over a mocking library** (e.g. Moq) for the
  Application-layer tests — `IGameRepository`/`IScoreboardRepository` are two
  tiny methods each, so a real (if trivial) in-memory implementation is both
  less code and more honest about behaviour than a mock's recorded
  expectations.
- **Single ASP.NET Core Web API project hosting the in-memory repositories**,
  rather than a fourth `TicTacToe.Infrastructure` class library — kept the
  solution to three source projects instead of four; the repositories still
  sit behind the same `IGameRepository`/`IScoreboardRepository` interfaces
  defined in `Application`, so this is a packaging choice, not a layering
  compromise (`Program.cs` is still the only file that references the
  concrete repository classes).
- **Angular standalone components (no NgModules)** — this is idiomatic for
  Angular 17 and keeps every feature component's dependencies explicit in its
  own `imports` array, rather than centralized in a module file that grows
  every time a new component is added.

## Assumptions made

Documented in full in README §10 ("Clarifications and Assumptions") rather
than duplicated here — in short: in-memory storage with one scoreboard per
running process, both `cellIndex` and `row`/`column` accepted on a move
request, deterministic (non-random) tie-breaking in the computer strategy,
and no authentication/multi-tenancy, since none of these were required by the
spec and each was called out explicitly rather than assumed silently.
