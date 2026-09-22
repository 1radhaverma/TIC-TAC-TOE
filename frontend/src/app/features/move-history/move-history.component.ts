import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Move } from '../../core/models/move.model';

/** Purely presentational: renders whatever move list it is given, oldest first, exactly as the backend returns it. */
@Component({
  selector: 'app-move-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './move-history.component.html',
  styleUrl: './move-history.component.scss'
})
export class MoveHistoryComponent {
  @Input() moves: Move[] = [];
}
