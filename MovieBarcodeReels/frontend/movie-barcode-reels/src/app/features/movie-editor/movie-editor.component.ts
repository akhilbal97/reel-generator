import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MovieApiService } from '../../core/services/movie-api.service';
import { DIFFICULTY_OPTIONS, GENRE_OPTIONS } from '../../core/models/movie.models';

@Component({
  selector: 'app-movie-editor',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './movie-editor.component.html',
  styleUrl: './movie-editor.component.scss',
})
export class MovieEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(MovieApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly difficulties = DIFFICULTY_OPTIONS;
  readonly genres = GENRE_OPTIONS;
  readonly saving = signal(false);
  readonly isEdit = signal(false);
  private movieId: number | null = null;

  barcodePreviewUrl: string | null = null;

  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(1888), Validators.max(2100)]],
    trivia: ['', Validators.required],
    genre: ['', Validators.required],
    difficulty: ['Medium', Validators.required],
    barcodeImagePath: [''],
    musicPath: [''],
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit.set(true);
      this.movieId = Number(idParam);
      this.loadMovie(this.movieId);
    }
  }

  loadMovie(id: number): void {
    this.api.getMovie(id).subscribe({
      next: (movie) => {
        this.form.patchValue({
          title: movie.title,
          year: movie.year,
          trivia: movie.trivia,
          genre: movie.genre,
          difficulty: movie.difficulty,
          barcodeImagePath: movie.barcodeImagePath ?? '',
          musicPath: movie.musicPath ?? '',
        });
      },
      error: () => {
        this.snackBar.open('Movie not found', 'Close', { duration: 3000 });
        this.router.navigate(['/movies']);
      },
    });
  }

  onBarcodeSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.barcodePreviewUrl = URL.createObjectURL(file);
    this.api.uploadFile(file).subscribe({
      next: (res) => {
        this.form.patchValue({ barcodeImagePath: res.relativePath });
        this.snackBar.open('Barcode uploaded', 'Close', { duration: 2500 });
      },
      error: () => this.snackBar.open('Upload failed', 'Close', { duration: 3000 }),
    });
  }

  onAudioSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.api.uploadFile(file).subscribe({
      next: (res) => {
        this.form.patchValue({ musicPath: res.relativePath });
        this.snackBar.open('Audio uploaded', 'Close', { duration: 2500 });
      },
      error: () => this.snackBar.open('Upload failed', 'Close', { duration: 3000 }),
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const payload = {
      title: value.title,
      year: value.year,
      trivia: value.trivia,
      genre: value.genre,
      difficulty: value.difficulty,
      barcodeImagePath: value.barcodeImagePath || null,
      musicPath: value.musicPath || null,
    };

    const request$ =
      this.isEdit() && this.movieId
        ? this.api.updateMovie(this.movieId, payload)
        : this.api.createMovie(payload);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.snackBar.open(this.isEdit() ? 'Movie updated' : 'Movie created', 'Close', { duration: 2500 });
        this.router.navigate(['/movies']);
      },
      error: () => {
        this.saving.set(false);
        this.snackBar.open('Save failed', 'Close', { duration: 3000 });
      },
    });
  }
}
