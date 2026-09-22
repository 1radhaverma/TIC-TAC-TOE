import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { GameService } from './core/services/game.service';
import { GameMode } from './core/models/enums';
import { GameState } from './core/models/game-state.model';
import { Scoreboard } from './core/models/scoreboard.model';
import { ModeSelectorComponent } from './features/mode-selector/mode-selector.component';
import { GameBoardComponent } from './features/game-board/game-board.component';
import { MoveHistoryComponent } from './features/move-history/move-history.component';
import { ScoreboardComponent } from './features/scoreboard/scoreboard.component';

/**
 * Root component and the app's only "smart" component: it is the sole place
 * that calls GameService, and the sole holder of the current GameState and
 * Scoreboard. Every feature component below it (ModeSelector, GameBoard,
 * MoveHistory, Scoreboard) is purely presentational - they receive data via
 * @Input and report player actions via @Output, and AppComponent is what turns
 * those actions into API calls and turns API responses back into @Input data.
 *
 * This one-way data flow is the same shape as the backend's layering: just as
 * GamesController never touches the repository directly (it goes through
 * IGameService), the feature components here never touch HttpClient directly -
 * they go through this component acting as their "service layer".
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ModeSelectorComponent, GameBoardComponent, MoveHistoryComponent, ScoreboardComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  gameState: GameState | null = null;
  scoreboard: Scoreboard = { xWins: 0, oWins: 0, draws: 0 };
  selectedMode: GameMode = 'TwoPlayer';
  errorMessage: string | null = null;

  constructor(private readonly gameService: GameService) {}

  ngOnInit(): void {
    // Load the scoreboard on startup so it is visible even before the first
    // game is created (e.g. after a page refresh, since nothing is persisted
    // in the browser - the backend is the only source of truth, by design).
    this.gameService.getScoreboard().subscribe({
      next: (scoreboard) => (this.scoreboard = scoreboard),
      error: () => {
        this.errorMessage = 'Could not reach the backend. Is the API running on http://localhost:5000?';
      }
    });
  }

  onModeChange(mode: GameMode): void {
    this.selectedMode = mode;
  }

  onStartNewGame(mode: GameMode): void {
    this.errorMessage = null;
    this.gameService.createGame(mode).subscribe({
      next: (state) => this.applyState(state),
      error: (err) => (this.errorMessage = this.extractError(err))
    });
  }

  onCellClicked(cellIndex: number): void {
    if (!this.gameState) {
      return;
    }

    this.errorMessage = null;
    this.gameService
      .submitMove(this.gameState.id, { player: this.gameState.currentPlayer, cellIndex })
      .subscribe({
        next: (state) => this.applyState(state),
        error: (err) => (this.errorMessage = this.extractError(err))
      });
  }

  onUndo(): void {
    if (!this.gameState) {
      return;
    }

    this.errorMessage = null;
    this.gameService.undoMove(this.gameState.id).subscribe({
      next: (state) => this.applyState(state),
      error: (err) => (this.errorMessage = this.extractError(err))
    });
  }

  onResetGame(): void {
    if (!this.gameState) {
      return;
    }

    this.errorMessage = null;
    this.gameService.resetGame(this.gameState.id).subscribe({
      next: (state) => this.applyState(state),
      error: (err) => (this.errorMessage = this.extractError(err))
    });
  }

  onResetScoreboard(): void {
    this.errorMessage = null;
    this.gameService.resetScoreboard().subscribe({
      next: (scoreboard) => (this.scoreboard = scoreboard),
      error: (err) => (this.errorMessage = this.extractError(err))
    });
  }

  /** Every successful API call returns a full GameState - this is the one place that snapshot is applied. */
  private applyState(state: GameState): void {
    this.gameState = state;
    this.scoreboard = state.scoreboard;
  }

  private extractError(err: unknown): string {
    if (err instanceof HttpErrorResponse) {
      const title = (err.error as { title?: string } | null)?.title;
      if (title) {
        return title;
      }

      if (err.status === 0) {
        return 'Could not reach the backend. Is the API running on http://localhost:5000?';
      }
    }

    return 'Something went wrong. Please try again.';
  }
}
