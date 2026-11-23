import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MovieApiService } from '../../core/services/movie-api.service';
import {
  DIFFICULTY_OPTIONS,
  GENRE_OPTIONS,
  GeneratedVideo,
  Movie,
} from '../../core/models/movie.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-dashboard',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(MovieApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly difficulties = DIFFICULTY_OPTIONS;
  readonly genres = GENRE_OPTIONS;

  readonly saving = signal(false);
  readonly generating = signal(false);
  readonly uploadingBarcode = signal(false);
  readonly uploadingAudio = signal(false);
  readonly savedMovie = signal<Movie | null>(null);
  readonly videos = signal<GeneratedVideo[]>([]);
  readonly previewVideoUrl = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(1888), Validators.max(2100)]],
    trivia: ['', Validators.required],
    genre: ['', Validators.required],
    difficulty: ['Medium', Validators.required],
    barcodeImagePath: [''],
    musicPath: [''],
  });

  barcodePreviewUrl: string | null = null;

  ngOnInit(): void {
    this.loadVideos();
  }

  loadVideos(): void {
    this.api.getVideos().subscribe({
      next: (videos) => this.videos.set(videos),
    });
  }

  onBarcodeSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingBarcode.set(true);
    this.barcodePreviewUrl = URL.createObjectURL(file);

    this.api.uploadFile(file).subscribe({
      next: (res) => {
        this.form.patchValue({ barcodeImagePath: res.relativePath });
        this.uploadingBarcode.set(false);
        this.snackBar.open('Barcode uploaded', 'Close', { duration: 2500 });
      },
      error: () => {
        this.uploadingBarcode.set(false);
        this.snackBar.open('Barcode upload failed', 'Close', { duration: 3000 });
      },
    });
  }

  onAudioSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingAudio.set(true);
    this.api.uploadFile(file).subscribe({
      next: (res) => {
        this.form.patchValue({ musicPath: res.relativePath });
        this.uploadingAudio.set(false);
        this.snackBar.open('Audio uploaded', 'Close', { duration: 2500 });
      },
      error: () => {
        this.uploadingAudio.set(false);
        this.snackBar.open('Audio upload failed', 'Close', { duration: 3000 });
      },
    });
  }

  saveAndGenerate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();

    this.api
      .createMovie({
        title: value.title,
        year: value.year,
        trivia: value.trivia,
        genre: value.genre,
        difficulty: value.difficulty,
        barcodeImagePath: value.barcodeImagePath || null,
        musicPath: value.musicPath || null,
      })
      .subscribe({
        next: (movie) => {
          this.savedMovie.set(movie);
          this.saving.set(false);
          this.generateReel(movie.id);
        },
        error: () => {
          this.saving.set(false);
          this.snackBar.open('Failed to save movie', 'Close', { duration: 3000 });
        },
      });
  }

  generateReel(movieId: number): void {
    this.generating.set(true);
    this.api.generateReel(movieId).subscribe({
      next: (res) => {
        this.generating.set(false);
        this.previewVideoUrl.set(`${environment.apiUrl}${res.downloadUrl}`);
        this.loadVideos();
        this.snackBar.open(
          res.usedPlaceholder
            ? 'Placeholder reel generated (install FFmpeg for full output)'
            : 'Reel generated successfully',
          'Close',
          { duration: 4000 },
        );
      },
      error: (err) => {
        this.generating.set(false);
        const message = err?.error?.message ?? 'Reel generation failed';
        this.snackBar.open(message, 'Close', { duration: 4000 });
      },
    });
  }

  videoDownloadUrl(video: GeneratedVideo): string {
    return `${environment.apiUrl}${video.downloadUrl}`;
  }
}
