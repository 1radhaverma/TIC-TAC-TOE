import { Mark } from './enums';

/** One entry in a game's move history. Row/Column arrive already 1-based from the backend, ready to display as-is. */
export interface Move {
  moveNumber: number;
  player: Mark;
  row: number;
  column: number;
  cellIndex: number;
}
