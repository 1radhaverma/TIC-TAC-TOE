# Tic Tac Toe — Angular + .NET Technical Assignment

A browser-based Tic Tac Toe game with an Angular frontend and a .NET Web API
backend. Two people can play against each other, or one person can play
against a simple rule-based computer opponent. The backend owns all game
rules, move history and the scoreboard; the frontend is a thin, stateless
renderer of whatever the backend last returned.

---

## 1. Project Overview

- **Two Player Mode** — two humans alternate as X and O on the same screen.
- **Vs Computer Mode** — the human is always X; the computer is always O and
  replies automatically, immediately after the human's move.
- Full win detection (rows, columns, diagonals) with the winning cells
  highlighted, draw detection, move history, Undo (mode-aware), Reset Game,
  and a session-level Scoreboard with its own Reset.
- The Angular app talks to the API exclusively over REST; nothing about game
  rules is duplicated in the frontend.

## 2. Tech Stack

| Layer      | Technology                                              |
|------------|----------------------------------------------------------|
| Frontend   | Angular 17 (standalone components), TypeScript, SCSS      |
| Backend    | .NET 8, ASP.NET Core Web API, C#                          |
| API style  | REST (JSON)                                               |
| Storage    | In-memory (thread-safe, per-process) — see §10             |
| Testing    | xUnit (backend), Jasmine/Karma (frontend)                  |
| Source control | GitHub                                                 |

## 3. Features Implemented

Every functional requirement in the problem statement is implemented:

- 3×3 board, click-to-play, cells lock once played.
- Two-player turn alternation; invalid moves never change whose turn it is.
- Win detection for all 8 lines; winner shown, winning cells highlighted,
  board locked, scoreboard updated.
- Draw detection; message shown, board locked, scoreboard updated.
- **Reset Game**: clears board/history/status, sets X to move, leaves the
  scoreboard untouched.
- **Move History**: move number, player, and 1-based row/column for every move.
- **Undo Last Move**: mode-aware (see §9 "Undo in Computer Mode"); disabled
  when there is nothing to undo.
- **Scoreboard**: X wins / O wins / Draws, updates exactly once per completed
  game, has its own independent **Reset Scoreboard**.
- **Vs Computer Mode**: computer is always O, moves automatically, and follows
  the required priority order (win → block → center → corner → any).

## 4. How to Run the Backend Locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd backend
dotnet restore
dotnet run --project src/TicTacToe.Api
```

The API listens on **http://localhost:5000** (fixed in
`src/TicTacToe.Api/Properties/launchSettings.json` so the frontend's
`environment.ts` always points at the right place). On first run in the
`Development` environment, Swagger UI is available at
**http://localhost:5000/swagger** for exploring/trying every endpoint.

## 5. How to Run the Frontend Locally

Requires [Node.js 18+](https://nodejs.org/) and npm.

```bash
cd frontend
npm install
npm start
```

This runs `ng serve`, which serves the app at **http://localhost:4200**. Make
sure the backend (§4) is already running — the frontend calls it directly and
does not proxy or mock any data.

> The backend's CORS policy only allows `http://localhost:4200` (see
> `Program.cs`). If you serve the frontend from a different port, update both
> `environment.ts` (frontend) and the CORS policy (backend).

## 6. API Endpoint Summary

All request/response bodies are JSON, `camelCase`.

| Method | Endpoint                     | Purpose                                             |
|--------|-------------------------------|------------------------------------------------------|
| POST   | `/api/games`                  | Create a new game session. Body: `{ "mode": "TwoPlayer" \| "VsComputer" }` |
| GET    | `/api/games/{id}`             | Get the current state of a game.                      |
| POST   | `/api/games/{id}/moves`       | Submit a move. Body: `{ "player": "X"\|"O", "cellIndex": 0-8 }` (or `"row"`/`"column"`, 0-based, instead of `cellIndex`). |
| POST   | `/api/games/{id}/undo`        | Undo the last move (or move pair — see §9).            |
| POST   | `/api/games/{id}/reset`       | Reset this game session. Scoreboard is untouched.      |
| GET    | `/api/scoreboard`             | Get the session scoreboard.                            |
| POST   | `/api/scoreboard/reset`       | Reset the scoreboard to zero.                          |

Every game endpoint returns the same **`GameStateDto`** shape:

```jsonc
{
  "id": "5b1e...",
  "board": ["X", null, "O", null, "X", null, null, null, null],
  "currentPlayer": "O",
  "mode": "VsComputer",
  "status": "InProgress",           // "InProgress" | "Won" | "Draw"
  "winner": null,                   // "X" | "O" | null
  "winningCells": null,             // e.g. [0, 4, 8] | null
  "moveHistory": [
    { "moveNumber": 1, "player": "X", "row": 1, "column": 1, "cellIndex": 0 }
  ],
  "canUndo": true,
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```

`row`/`column` in `moveHistory` are **1-based** for display, matching the
spec's example table; the request body's optional `row`/`column` fields are
**0-based** (0–2), matching the domain's internal indexing.

Full interactive documentation (with schemas and a "Try it out" button) is
generated by Swagger — see §4.

## 7. How to Run Tests

**Backend** (from `backend/`):

```bash
dotnet test
```

Covers, at minimum, everything the assignment's testing checklist asks for:
valid/invalid moves, turn switching, row/column/diagonal wins, draw, reset,
undo in both modes, scoreboard updates (including "exactly once"), computer
move selection at every priority rung, and rejecting moves after completion.
See `backend/tests/TicTacToe.Domain.Tests` (pure game rules) and
`backend/tests/TicTacToe.Application.Tests` (orchestration, using
hand-written fakes for the repositories — no mocking library needed for such
small interfaces).

**Frontend** (from `frontend/`):

```bash
npm test
```

Covers `GameService`'s HTTP calls (via `HttpClientTestingModule`) and
`GameBoardComponent`'s/`AppComponent`'s rendering and click-handling logic.

## 8. AI Tools and Prompt Summary

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

## 8. AI Tools and Prompt Summary

AI assistance was used during development to reason through some of the
trickier design decisions in the game logic. A summary of the key prompts
and the resulting reasoning:

**"How should Tic Tac Toe win and draw conditions be handled? Explain every
step, follow SOLID principles and OOP concepts."**

A 3×3 board only has 8 possible ways to win: 3 rows, 3 columns, 2 diagonals.
Rather than writing a separate check for each, they're stored as one array
of index-triplets (e.g. `{0,1,2}` for the top row) and checked in a loop.
After every move: evaluate the board against those 8 lines — if any line has
the same non-empty mark in all three cells, the game is **Won** (store the
winner and the winning cells so the UI can highlight them); if nobody won
and every cell is filled, it's a **Draw**; otherwise the game stays
**InProgress** and the turn passes to the other player. This logic lives
behind an `IWinChecker` interface rather than being hard-coded inside
`Game` — `Game` only knows it can call `Evaluate(board)`, not how the
answer is produced (Dependency Inversion), which also means the win rule
could be swapped later without touching `Game` at all (Open/Closed).
`WinChecker` itself does exactly one job (Single Responsibility).

**"What validations should be performed when a player submits a move?"**

Four checks, run in order, before anything is mutated: (1) is the game
already over — reject if `Won`/`Draw`; (2) is the cell index actually on the
board (0–8); (3) is it this player's turn; (4) is the cell already occupied.
Any failure throws immediately and the board is left completely untouched —
there's no scenario where a rejected move partially changes state.

**"How can undo behavior be handled differently for two-player and computer
modes?"**

Two Player Mode: Undo removes exactly the one most recent move. Vs Computer
Mode is trickier, since the computer replies automatically in the same
request right after the human moves — so Undo needs to remove the human
move and the computer's reply together, as a pair, to land back on a
position the human actually chose. The one edge case: if the human's move
ends the game before the computer gets a turn, Undo removes only that single
human move. This is implemented by checking which player made the *last*
recorded move (not by assuming move counts are always even), which handles
the edge case without any special-casing.

**"What simple strategy can be used for the computer opponent?"**

A rule-based priority list rather than a search algorithm: (1) win
immediately if possible; (2) otherwise block the opponent's winning move;
(3) otherwise take the center (part of 4 winning lines); (4) otherwise take
a corner (part of 3 lines); (5) otherwise take any remaining cell. Steps 1
and 2 reuse the same win-detection logic from the question above instead of
duplicating "what counts as a line" — each empty cell is tried as a
hypothetical move and evaluated, then discarded if it doesn't win.

**"What core scenarios should be tested for the game?"**

Valid moves update the board and switch the turn; invalid moves (wrong
turn, occupied cell, out-of-range index, move after completion) are
rejected and leave state unchanged; each of the 8 winning lines is
detected; a full board with no winner is a draw; Reset clears the
board/history but not the scoreboard; Undo behaves correctly in both modes
(including the edge case above) and is rejected when there's nothing to
undo or the game has finished; the scoreboard updates exactly once per
completed game; the computer picks the correct cell at every priority rung.

**"How can winning combinations be checked without writing separate logic
for every row, column, and diagonal?"**

Represent the board as a flat list of 9 cells (indices 0–8) instead of a 2D
grid. The 8 winning lines become 8 sets of 3 indices — `{0,1,2} {3,4,5}
{6,7,8}` (rows), `{0,3,6} {1,4,7} {2,5,8}` (columns), `{0,4,8} {2,4,6}`
(diagonals) — stored as one array-of-arrays and checked with a single loop:
for each line, if the first cell isn't empty and the other two match it,
that's a win. One small piece of logic, reused 8 times.

## 9. Design Decisions

- **Layered / Clean-ish architecture** — `TicTacToe.Domain` (entities + rules,
  zero dependencies) → `TicTacToe.Application` (use cases, DTOs, depends only
  on Domain) → `TicTacToe.Api` (HTTP, DI composition root, depends on both).
  Dependencies point inward: Domain never references Application or Api.
  This is what lets the domain and application layers be unit-tested with no
  web server involved at all.
- **In-memory storage** — chosen because the assignment explicitly allows it
  and this is a local review exercise, not a production deployment. Both
  repositories are hidden behind interfaces (`IGameRepository`,
  `IScoreboardRepository`) defined in the Application layer, so a SQLite (or
  any other) implementation could be dropped in later by adding one class and
  changing one line of DI registration in `Program.cs` — nothing in Domain or
  Application would need to change.
- **Undo after completion → Option A (Disabled)**. Once a game is `Won` or
  `Draw`, Undo is disabled (`InvalidGameOperationException` → HTTP 409); the
  scoreboard entry for that game is therefore always final. This was chosen
  over Option B (allow Undo, then reverse the scoreboard) because it removes
  an entire class of bugs (double-decrementing, undoing into a state that
  contradicts a scoreboard that already "happened") for a local review
  exercise where a completed game can simply be reset to play again.
- **Undo behavior in Computer Mode** — removes the computer's move together
  with the human move immediately before it, *unless* the computer has not
  replied yet (i.e. the human's move ended the game before the computer got a
  turn), in which case only that single human move is removed. Implemented by
  checking which player made the *last* recorded move, not by assuming move
  counts are always even — see the comment on `Game.DetermineUndoCount()`.
- **Undo/Reset rebuild rather than reverse** — both replay the surviving move
  history onto a brand-new `Board` and re-run the win/draw check, rather than
  trying to algebraically "undo" a win calculation. Simpler, and eliminates
  an entire category of state-drift bugs.
- **Move requests accept `cellIndex` *or* `row`+`column`** — the spec allows
  either shape ("Row and column, or cell index"), so `MoveRequestDto` accepts
  both and `GameService.ResolveCellIndex` reconciles them before the request
  ever reaches the domain layer, which only ever deals in flat indices.
- **Computer strategy is a Strategy-pattern class, not an `if` chain buried in
  the service** — `IComputerPlayerStrategy` / `BasicComputerStrategy` reuses
  the exact same `IWinChecker` the rest of the game uses (rather than
  duplicating "what is a winning line"), and can be swapped for a smarter
  implementation later without touching `GameService`.
- **Frontend is intentionally "dumb"** — every component other than
  `AppComponent` is purely presentational (`@Input`/`@Output` only, no
  `HttpClient`, no local copies of game state). `AppComponent` is the only
  place that calls `GameService` and the only place that holds a `GameState`.
  This mirrors "Clarification 1: Backend State Ownership" on the client side:
  there is nowhere for client and server state to drift apart because the
  client never computes any of it.

### SOLID & OOP, Concretely

Rather than list the principles abstractly, here is where each one actually
shows up in this codebase (see also the inline comments in the referenced
files — the aim throughout was to explain the reasoning at the point it
applies, not just here):

| Principle | Where |
|---|---|
| **Single Responsibility** | `Board` only stores/queries cells. `WinChecker` only knows the 8 winning lines. `Scoreboard` only tallies results. `GameMapper` is the only place domain↔DTO conversion happens. `ExceptionHandlingMiddleware` is the only place exceptions become HTTP status codes. |
| **Open/Closed** | `IWinChecker` and `IComputerPlayerStrategy` let the win rule or the computer's intelligence be replaced (e.g. a Minimax strategy) by adding a new class and changing one DI registration — no existing class needs editing. |
| **Liskov Substitution** | Any `IGameRepository`/`IScoreboardRepository`/`IWinChecker`/`IComputerPlayerStrategy` implementation is fully interchangeable — `GameService` and `Game` only ever call the interface's contract, which the tests prove by substituting hand-written fakes for the real in-memory repositories. |
| **Interface Segregation** | `IGameService` and `IScoreboardService` are separate interfaces (and separate controllers) even though one backing store could theoretically serve both — `ScoreboardController` has no reason to see game-management methods, or vice versa. |
| **Dependency Inversion** | `GameService`, `Game`, and `BasicComputerStrategy` all depend only on interfaces passed into their constructors/methods, never on `new SomeConcreteClass()`. `Program.cs` is the single composition root where concrete types (`InMemoryGameRepository`, `WinChecker`, `BasicComputerStrategy`) are wired to the abstractions everything else depends on. |
| **Encapsulation** | Every domain entity (`Board`, `Game`, `Scoreboard`) exposes behaviour methods (`PlaceMark`, `ApplyMove`, `RecordResult`) instead of public setters — callers cannot put an entity into an invalid state by assigning a property directly. |
| **Abstraction** | `IWinChecker.Evaluate(board)` and `IComputerPlayerStrategy.ChooseMove(...)` are the *only* things callers need to know; how a win is detected or a move is chosen is entirely hidden behind those two calls. |
| **Polymorphism** | Any current or future `IComputerPlayerStrategy` (or `IWinChecker`) implementation is used identically by `GameService`/`Game` via the interface type — swapping `BasicComputerStrategy` for a hypothetical `MinimaxComputerStrategy` requires no caller changes. |
| **Immutability** | `Move` is a C# `record` — a played move is never mutated, only replayed (see `Board.ReplayMoves`), which is what makes Undo simple and safe. |

## 10. Clarifications and Assumptions

- **Storage**: in-memory, as explicitly permitted. All state — every game and
  the scoreboard — lives only in the running API process's memory and is
  lost on restart. There is exactly one shared scoreboard per running
  backend (not per game, not per browser tab).
- **Scoreboard/Undo clarification**: Option A (Disable Undo After Completion)
  was chosen — see §9 for the reasoning.
- **Move request shape**: both `cellIndex` and `row`+`column` are accepted
  (see §6); the frontend always sends `cellIndex`.
- **Computer strategy determinism**: at the "take a corner" and "take any
  cell" rungs, the *first* qualifying cell (by ascending index) is chosen.
  The spec does not require randomness, so behaviour is fully deterministic
  and easy to unit test.
- **No authentication/multi-tenancy**: this is a single local backend
  instance for one reviewer session; games are identified only by GUID, with
  no per-user scoping, matching the scope of a local technical assignment.
- **CORS is locked to `http://localhost:4200`** for this same reason.

## 11. Known Limitations

- State does not survive an API restart (by design — see §10).
- No persistence option is wired up out of the box beyond in-memory (SQLite
  was optional per the spec; the repository abstraction is ready for it, but
  no concrete SQLite implementation is included, to keep the reviewed surface
  area focused).
- The computer opponent implements the specified priority list (win → block →
  center → corner → any); it is not a full Minimax/unbeatable AI, which was
  not required by the spec ("Basic Computer Mode").
- No websocket/live-sync between multiple browser tabs viewing the same game
  — each client only sees the state returned by its own last request.

## 12. Future Improvements

- Swap `InMemoryGameRepository`/`InMemoryScoreboardRepository` for a SQLite-
  backed implementation of the same interfaces for durability across restarts.
- Add a `MinimaxComputerStrategy` (unbeatable play) behind the existing
  `IComputerPlayerStrategy` interface, selectable per game.
- Persist per-user scoreboards (would need lightweight auth/session
  identification, currently out of scope).
- Add end-to-end tests (e.g. Playwright/Cypress) exercising the real running
  backend + frontend together, complementing the current unit-test layers.

---

## Project Structure

```
tic-tac-toe/
├── backend/
│   ├── TicTacToe.sln
│   ├── src/
│   │   ├── TicTacToe.Domain/          # Entities, enums, rules, strategies — zero dependencies
│   │   ├── TicTacToe.Application/     # Interfaces, DTOs, services (use cases) — depends only on Domain
│   │   └── TicTacToe.Api/             # Controllers, DI composition root, in-memory persistence
│   └── tests/
│       ├── TicTacToe.Domain.Tests/
│       └── TicTacToe.Application.Tests/
├── frontend/
│   └── src/app/
│       ├── core/                      # models + GameService (the only class that calls HttpClient)
│       └── features/                  # mode-selector, game-board, move-history, scoreboard (presentational)
├── README.md
└── PROMPTS.md
```
