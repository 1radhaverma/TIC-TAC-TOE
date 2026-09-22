import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let component: AppComponent;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiUrl}/api`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, HttpClientTestingModule]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    // ngOnInit fires an initial scoreboard fetch - satisfy it so it does not leak into other tests.
    httpMock.expectOne(`${baseUrl}/scoreboard`).flush({ xWins: 0, oWins: 0, draws: 0 });
  });

  afterEach(() => httpMock.verify());

  it('renders the title', () => {
    const heading: HTMLElement = fixture.nativeElement.querySelector('h1');
    expect(heading.textContent).toContain('Tic Tac Toe');
  });

  it('shows the "no active game" hint before a game is created', () => {
    expect(fixture.nativeElement.querySelector('.app__hint')).toBeTruthy();
  });

  it('creating a game applies the returned state and hides the hint', () => {
    component.onStartNewGame('TwoPlayer');

    const req = httpMock.expectOne(`${baseUrl}/games`);
    expect(req.request.body).toEqual({ mode: 'TwoPlayer' });
    req.flush({
      id: 'game-1',
      board: Array(9).fill(null),
      currentPlayer: 'X',
      mode: 'TwoPlayer',
      status: 'InProgress',
      winner: null,
      winningCells: null,
      moveHistory: [],
      canUndo: false,
      scoreboard: { xWins: 0, oWins: 0, draws: 0 }
    });

    fixture.detectChanges();

    expect(component.gameState?.id).toBe('game-1');
    expect(fixture.nativeElement.querySelector('.app__hint')).toBeFalsy();
  });
});
