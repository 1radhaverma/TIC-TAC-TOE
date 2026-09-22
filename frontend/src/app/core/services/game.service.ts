import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GameMode } from '../models/enums';
import { GameState } from '../models/game-state.model';
import { MoveRequest } from '../models/move-request.model';
import { Scoreboard } from '../models/scoreboard.model';

/**
 * Thin wrapper around the seven REST endpoints the backend exposes.
 *
 * SOLID / OOP notes:
 * - <b>Single Responsibility</b>: this class only knows how to build and send
 *   HTTP requests. It has no idea what a "winning cell" is or how Undo works -
 *   that logic lives entirely on the backend; this service just relays it.
 * - Every component that needs game data depends on this one injectable
 *   service (via Angular's DI, mirroring the backend's constructor injection)
 *   rather than calling HttpClient directly - if the API's base path or
 *   request shapes ever changed, this is the only file that would need to.
 */
@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly baseUrl = `${environment.apiUrl}/api`;

  constructor(private readonly http: HttpClient) {}

  /** POST /api/games */
  createGame(mode: GameMode): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games`, { mode });
  }

  /** GET /api/games/{id} */
  getGame(gameId: string): Observable<GameState> {
    return this.http.get<GameState>(`${this.baseUrl}/games/${gameId}`);
  }

  /** POST /api/games/{id}/moves */
  submitMove(gameId: string, request: MoveRequest): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/moves`, request);
  }

  /** POST /api/games/{id}/undo */
  undoMove(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/undo`, {});
  }

  /** POST /api/games/{id}/reset */
  resetGame(gameId: string): Observable<GameState> {
    return this.http.post<GameState>(`${this.baseUrl}/games/${gameId}/reset`, {});
  }

  /** GET /api/scoreboard */
  getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${this.baseUrl}/scoreboard`);
  }

  /** POST /api/scoreboard/reset */
  resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${this.baseUrl}/scoreboard/reset`, {});
  }
}
