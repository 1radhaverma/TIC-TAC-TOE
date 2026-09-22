import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GameState } from '../../core/models/game-state.model';

/**
 * Renders one game's board and status entirely from the <see>GameState</see>
 * it is given - it holds no state of its own (no local "board" array, no
 * local "current player" field). That mirrors "Clarification 1: Backend State
 * Ownership" in the README on the Angular side: this component never decides
 * whether a move is legal, it only ever displays what the backend last said
 * and forwards the player's intent (a cell click, Undo, Reset) back up to
 * <see>AppComponent</see>, which is the only place that talks to GameService.
 */
@Component({
  selector: 'app-game-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game-board.component.html',
  styleUrl: './game-board.component.scss'
})
export class GameBoardComponent {
  @Input({ required: true }) state!: GameState;

  /** Emits the 0-8 index of the cell the player clicked - only ever for a cell that is actually clickable. */
  @Output() cellClicked = new EventEmitter<number>();
  @Output() undo = new EventEmitter<void>();
  @Output() resetGame = new EventEmitter<void>();

  get statusMessage(): string {
    switch (this.state.status) {
      case 'Won':
        return `Player ${this.state.winner} wins!`;
      case 'Draw':
        return "It's a draw!";
      default:
        return `${this.state.currentPlayer}'s turn`;
    }
  }

  get modeLabel(): string {
    return this.state.mode === 'VsComputer' ? 'Vs Computer' : 'Two Player';
  }

  isWinningCell(index: number): boolean {
    return this.state.winningCells?.includes(index) ?? false;
  }

  /** A cell can be played only while the game is still in progress and the cell is empty. */
  isCellClickable(index: number): boolean {
    return this.state.status === 'InProgress' && this.state.board[index] === null;
  }

  onCellClick(index: number): void {
    if (this.isCellClickable(index)) {
      this.cellClicked.emit(index);
    }
  }

  /** 1-based row for a flat cell index - used only for the accessible label on each cell. */
  cellRow(index: number): number {
    return Math.floor(index / 3) + 1;
  }

  /** 1-based column for a flat cell index - used only for the accessible label on each cell. */
  cellColumn(index: number): number {
    return (index % 3) + 1;
  }
}
