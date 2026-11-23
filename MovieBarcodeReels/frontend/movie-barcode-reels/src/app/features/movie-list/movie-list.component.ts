import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MovieApiService } from '../../core/services/movie-api.service';
import { Movie } from '../../core/models/movie.models';

@Component({
  selector: 'app-movie-list',
  imports: [
    RouterLink,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSnackBarModule,
  ],
  templateUrl: './movie-list.component.html',
  styleUrl: './movie-list.component.scss',
})
export class MovieListComponent implements OnInit {
  private readonly api = inject(MovieApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly movies = signal<Movie[]>([]);
  readonly displayedColumns = ['title', 'year', 'genre', 'difficulty', 'createdDate', 'actions'];

  ngOnInit(): void {
    this.loadMovies();
  }

  loadMovies(): void {
    this.api.getMovies().subscribe({
      next: (movies) => this.movies.set(movies),
      error: () => this.snackBar.open('Failed to load movies', 'Close', { duration: 3000 }),
    });
  }

  deleteMovie(id: number): void {
    if (!confirm('Delete this movie?')) return;

    this.api.deleteMovie(id).subscribe({
      next: () => {
        this.movies.update((list) => list.filter((m) => m.id !== id));
        this.snackBar.open('Movie deleted', 'Close', { duration: 2500 });
      },
      error: () => this.snackBar.open('Delete failed', 'Close', { duration: 3000 }),
    });
  }
}
