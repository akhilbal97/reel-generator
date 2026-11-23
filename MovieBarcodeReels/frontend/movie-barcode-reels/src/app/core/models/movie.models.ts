export interface Movie {
  id: number;
  title: string;
  year: number;
  trivia: string;
  genre: string;
  difficulty: string;
  barcodeImagePath: string | null;
  musicPath: string | null;
  createdDate: string;
}

export interface CreateMovieRequest {
  title: string;
  year: number;
  trivia: string;
  genre: string;
  difficulty: string;
  barcodeImagePath: string | null;
  musicPath: string | null;
}

export interface UpdateMovieRequest extends CreateMovieRequest {}

export interface UploadResponse {
  relativePath: string;
  fileName: string;
  sizeBytes: number;
}

export interface GenerateReelResponse {
  videoId: number;
  movieId: number;
  downloadUrl: string;
  relativePath: string;
  durationMs: number;
  usedPlaceholder: boolean;
}

export interface GeneratedVideo {
  id: number;
  movieId: number;
  movieTitle: string;
  relativePath: string;
  downloadUrl: string;
  createdDate: string;
  durationMs: number;
  usedPlaceholder: boolean;
}

export interface ApiErrorResponse {
  message: string;
  detail?: string;
}

export const DIFFICULTY_OPTIONS = ['Easy', 'Medium', 'Hard'] as const;

export const GENRE_OPTIONS = [
  'Action',
  'Adventure',
  'Animation',
  'Comedy',
  'Crime',
  'Drama',
  'Fantasy',
  'Horror',
  'Mystery',
  'Romance',
  'Sci-Fi',
  'Thriller',
] as const;
