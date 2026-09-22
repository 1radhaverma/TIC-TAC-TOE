import { GameMode, GameStatus, Mark } from './enums';
import { Move } from './move.model';
import { Scoreboard } from './scoreboard.model';

/**
 * Full snapshot of one game, exactly as GameMapper.ToGameStateDto builds it on
 * the backend. Every component in this app renders purely from this object -
 * none of them keep their own copy of "whose turn is it" or "has someone won" -
 * which is what makes the backend the single source of truth in practice, not
 * just in the README (see "Clarification 1: Backend State Ownership").
 */
export interface GameState {
  id: string;
  /** 9 entries, index 0-8; each is "X", "O" or null for an empty cell. */
  board: (Mark | null)[];
  currentPlayer: Mark;
  mode: GameMode;
  status: GameStatus;
  winner: Mark | null;
  winningCells: number[] | null;
  moveHistory: Move[];
  canUndo: boolean;
  scoreboard: Scoreboard;
}
