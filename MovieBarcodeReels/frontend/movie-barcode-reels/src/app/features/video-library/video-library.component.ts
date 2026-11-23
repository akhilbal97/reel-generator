import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MovieApiService } from '../../core/services/movie-api.service';
import { GeneratedVideo } from '../../core/models/movie.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-video-library',
  imports: [DatePipe, MatButtonModule, MatIconModule, MatSnackBarModule],
  templateUrl: './video-library.component.html',
  styleUrl: './video-library.component.scss',
})
export class VideoLibraryComponent implements OnInit {
  private readonly api = inject(MovieApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly videos = signal<GeneratedVideo[]>([]);
  readonly selectedVideoUrl = signal<string | null>(null);

  ngOnInit(): void {
    this.loadVideos();
  }

  loadVideos(): void {
    this.api.getVideos().subscribe({
      next: (videos) => this.videos.set(videos),
      error: () => this.snackBar.open('Failed to load videos', 'Close', { duration: 3000 }),
    });
  }

  preview(video: GeneratedVideo): void {
    this.selectedVideoUrl.set(`${environment.apiUrl}${video.downloadUrl}`);
  }

  downloadUrl(video: GeneratedVideo): string {
    return `${environment.apiUrl}${video.downloadUrl}`;
  }

  formatDuration(ms: number): string {
    const seconds = Math.round(ms / 1000);
    return `${seconds}s`;
  }
}
