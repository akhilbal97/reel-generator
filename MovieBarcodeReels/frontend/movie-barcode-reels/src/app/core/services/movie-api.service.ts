import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateMovieRequest,
  GeneratedVideo,
  GenerateReelResponse,
  Movie,
  UpdateMovieRequest,
  UploadResponse,
} from '../models/movie.models';

@Injectable({ providedIn: 'root' })
export class MovieApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getMovies(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.baseUrl}/api/movies`);
  }

  getMovie(id: number): Observable<Movie> {
    return this.http.get<Movie>(`${this.baseUrl}/api/movies/${id}`);
  }

  createMovie(request: CreateMovieRequest): Observable<Movie> {
    return this.http.post<Movie>(`${this.baseUrl}/api/movies`, request);
  }

  updateMovie(id: number, request: UpdateMovieRequest): Observable<Movie> {
    return this.http.put<Movie>(`${this.baseUrl}/api/movies/${id}`, request);
  }

  deleteMovie(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/movies/${id}`);
  }

  uploadFile(file: File): Observable<UploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadResponse>(`${this.baseUrl}/api/upload`, formData);
  }

  generateReel(movieId: number): Observable<GenerateReelResponse> {
    return this.http.post<GenerateReelResponse>(`${this.baseUrl}/api/reels/generate`, {
      movieId,
    });
  }

  getVideos(): Observable<GeneratedVideo[]> {
    return this.http.get<GeneratedVideo[]>(`${this.baseUrl}/api/videos`);
  }

  getVideoDownloadUrl(videoId: number): string {
    return `${this.baseUrl}/api/videos/${videoId}/download`;
  }
}
