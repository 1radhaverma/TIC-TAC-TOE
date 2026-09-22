import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameBoardComponent } from './game-board.component';
import { GameState } from '../../core/models/game-state.model';

describe('GameBoardComponent', () => {
  let fixture: ComponentFixture<GameBoardComponent>;
  let component: GameBoardComponent;

  const baseState: GameState = {
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
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameBoardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(GameBoardComponent);
    component = fixture.componentInstance;
    component.state = { ...baseState };
    fixture.detectChanges();
  });

  it('shows whose turn it is while the game is in progress', () => {
    expect(component.statusMessage).toBe("X's turn");
  });

  it('shows the winner once the game is won', () => {
    component.state = { ...baseState, status: 'Won', winner: 'O', winningCells: [0, 4, 8] };
    fixture.detectChanges();

    expect(component.statusMessage).toBe('Player O wins!');
    expect(component.isWinningCell(4)).toBeTrue();
    expect(component.isWinningCell(1)).toBeFalse();
  });

  it('shows a draw message when the game is drawn', () => {
    component.state = { ...baseState, status: 'Draw' };
    fixture.detectChanges();

    expect(component.statusMessage).toBe("It's a draw!");
  });

  it('emits cellClicked only for cells that are empty and the game is in progress', () => {
    const emitted: number[] = [];
    component.cellClicked.subscribe((index) => emitted.push(index));

    component.onCellClick(0); // empty -> should emit
    component.state = { ...baseState, board: ['X', null, null, null, null, null, null, null, null] };
    component.onCellClick(0); // now occupied -> should not emit

    expect(emitted).toEqual([0]);
  });

  it('does not allow clicks once the game has finished', () => {
    component.state = { ...baseState, status: 'Won', winner: 'X', winningCells: [0, 1, 2] };
    const emitted: number[] = [];
    component.cellClicked.subscribe((index) => emitted.push(index));

    component.onCellClick(5);

    expect(emitted).toEqual([]);
  });

  it('renders 9 cell buttons', () => {
    const buttons = fixture.nativeElement.querySelectorAll('.game-board__cell');
    expect(buttons.length).toBe(9);
  });
});
