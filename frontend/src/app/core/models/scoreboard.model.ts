/** Session-level tally, mirroring the backend's ScoreboardDto. */
export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}
