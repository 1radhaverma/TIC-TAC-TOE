import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GameMode } from '../../core/models/enums';

/**
 * Presentational component: it only knows how to display two mode options and
 * a "New Game" button, and to emit events when the player interacts with them.
 * It never calls GameService itself - AppComponent owns that responsibility -
 * which is what makes this component trivial to reuse or restyle without
 * touching any HTTP logic (Single Responsibility, applied to a UI component).
 */
@Component({
  selector: 'app-mode-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mode-selector.component.html',
  styleUrl: './mode-selector.component.scss'
})
export class ModeSelectorComponent {
  /** Which mode is currently highlighted/selected in the UI. */
  @Input() selectedMode: GameMode = 'TwoPlayer';

  /** True while a game is already in progress - used to label the button "New Game" vs "Start Game". */
  @Input() hasActiveGame = false;

  /** Fired whenever the player picks a different mode (before starting a game). */
  @Output() modeChange = new EventEmitter<GameMode>();

  /** Fired when the player clicks the start/new game button, carrying the currently selected mode. */
  @Output() startGame = new EventEmitter<GameMode>();

  selectMode(mode: GameMode): void {
    this.modeChange.emit(mode);
  }

  start(): void {
    this.startGame.emit(this.selectedMode);
  }
}
