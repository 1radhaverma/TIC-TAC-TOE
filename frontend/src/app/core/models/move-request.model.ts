import { Mark } from './enums';

/**
 * Request body for POST /api/games/{id}/moves. This app always sends
 * `cellIndex` (the board component works in flat 0-8 indices throughout), but
 * the field is optional here - and row/column are included - because the
 * backend accepts either shape (see MoveRequestDto on the backend).
 */
export interface MoveRequest {
  player: Mark;
  cellIndex?: number;
  row?: number;
  column?: number;
}
