/**
 * These three types mirror the backend's `Mark`, `GameMode` and `GameStatus`
 * enums exactly (see TicTacToe.Domain/Enums on the backend and GameMapper's
 * string conversions). Keeping them as string literal unions - rather than
 * TypeScript `enum`s - means they serialise/deserialise to and from the JSON
 * the API sends with zero conversion code on this side.
 */

/** A player's mark, and the contents of an occupied board cell. */
export type Mark = 'X' | 'O';

/** Whether a game is two humans, or a human (X) versus the computer (O). */
export type GameMode = 'TwoPlayer' | 'VsComputer';

/** A game session's lifecycle status. */
export type GameStatus = 'InProgress' | 'Won' | 'Draw';
