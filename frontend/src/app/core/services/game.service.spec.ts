import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { GameService } from './game.service';
import { GameState } from '../models/game-state.model';
import { environment } from '../../../environments/environment';

describe('GameService', () => {
  let service: GameService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiUrl}/api`;

  const sampleGameState: GameState = {
    id: 'a1b2c3',
    board: Array(9).fill(null),
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: null,
    moveHistory: [],
    canUndo: false,
    scoreboard: { xWins: 0, oWins: 0, draws: 0 }
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [GameService]
    });

    service = TestBed.inject(GameService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('createGame posts the mode to /api/games', () => {
    service.createGame('VsComputer').subscribe((state) => expect(state).toEqual(sampleGameState));

    const req = httpMock.expectOne(`${baseUrl}/games`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ mode: 'VsComputer' });
    req.flush(sampleGameState);
  });

  it('getGame calls GET /api/games/{id}', () => {
    service.getGame('a1b2c3').subscribe((state) => expect(state).toEqual(sampleGameState));

    const req = httpMock.expectOne(`${baseUrl}/games/a1b2c3`);
    expect(req.request.method).toBe('GET');
    req.flush(sampleGameState);
  });

  it('submitMove posts the move to /api/games/{id}/moves', () => {
    service.submitMove('a1b2c3', { player: 'X', cellIndex: 4 }).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/games/a1b2c3/moves`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ player: 'X', cellIndex: 4 });
    req.flush(sampleGameState);
  });

  it('undoMove posts to /api/games/{id}/undo', () => {
    service.undoMove('a1b2c3').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/games/a1b2c3/undo`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleGameState);
  });

  it('resetGame posts to /api/games/{id}/reset', () => {
    service.resetGame('a1b2c3').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/games/a1b2c3/reset`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleGameState);
  });

  it('getScoreboard calls GET /api/scoreboard', () => {
    const scoreboard = { xWins: 2, oWins: 1, draws: 0 };
    service.getScoreboard().subscribe((result) => expect(result).toEqual(scoreboard));

    const req = httpMock.expectOne(`${baseUrl}/scoreboard`);
    expect(req.request.method).toBe('GET');
    req.flush(scoreboard);
  });

  it('resetScoreboard posts to /api/scoreboard/reset', () => {
    const scoreboard = { xWins: 0, oWins: 0, draws: 0 };
    service.resetScoreboard().subscribe((result) => expect(result).toEqual(scoreboard));

    const req = httpMock.expectOne(`${baseUrl}/scoreboard/reset`);
    expect(req.request.method).toBe('POST');
    req.flush(scoreboard);
  });
});
