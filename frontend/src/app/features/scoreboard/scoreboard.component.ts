import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Scoreboard } from '../../core/models/scoreboard.model';

/**
 * Displays the session scoreboard the backend serves (see "the scoreboard
 * should be served by the backend" in the spec) and forwards a "reset"
 * click upward - it never zeroes the counters itself, since the backend is
 * the only source of truth for them.
 */
@Component({
  selector: 'app-scoreboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scoreboard.component.html',
  styleUrl: './scoreboard.component.scss'
})
export class ScoreboardComponent {
  @Input({ required: true }) scoreboard!: Scoreboard;
  @Output() resetScoreboard = new EventEmitter<void>();
}
