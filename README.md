# Movie Barcode Reel Generator

A full-stack MVP for creating Instagram Reels and YouTube Shorts from movie barcode images.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Angular 22, Angular Material, Standalone Components, Reactive Forms |
| Backend | ASP.NET Core Web API (.NET 10) |
| Database | SQLite + Entity Framework Core |
| Storage | Local filesystem (`assets/uploads`, `assets/generated`) |
| Video | FFmpeg (with placeholder fallback) |

## Project Structure

```
.
├── README.md
├── assets/                         # Source barcode images
├── assets2/                        # Additional barcode images
└── MovieBarcodeReels/
    ├── frontend/movie-barcode-reels/   # Angular app
    ├── backend/MovieBarcodeReels.Api/  # ASP.NET Core API
    └── assets/
        ├── uploads/                    # Uploaded barcode images & audio
        └── generated/                  # Generated MP4 reels
```

## Prerequisites

- [.NET SDK 10+](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- [FFmpeg](https://ffmpeg.org/download.html) (optional — placeholder output used if unavailable)

## Quick Start

### 1. Backend

```bash
cd MovieBarcodeReels/backend/MovieBarcodeReels.Api
dotnet restore
dotnet run
```

The API starts at **http://localhost:5000**.

In Development, Swagger UI is available at **http://localhost:5000/swagger**.

Database migrations run automatically on startup. The SQLite database is created at `MovieBarcodeReels/Data/moviebarcode.db`.

### 2. Frontend

```bash
cd MovieBarcodeReels/frontend/movie-barcode-reels
npm install
npm start
```

The app opens at **http://localhost:4200**.

## Database Migrations

Migrations are applied automatically when the API starts. To create a new migration manually:

```bash
cd MovieBarcodeReels/backend/MovieBarcodeReels.Api
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/movies` | List all movies |
| GET | `/api/movies/{id}` | Get movie by ID |
| POST | `/api/movies` | Create movie |
| PUT | `/api/movies/{id}` | Update movie |
| DELETE | `/api/movies/{id}` | Delete movie |
| POST | `/api/upload` | Upload PNG/JPEG/JPG/MP3 |
| POST | `/api/reels/generate` | Generate reel for a movie |
| GET | `/api/videos` | List generated videos |
| GET | `/api/videos/{id}/download` | Download generated video |

## FFmpeg Setup

1. Install FFmpeg and ensure `ffmpeg` is on your PATH.
   - **Windows:** `winget install ffmpeg` or download from [ffmpeg.org](https://ffmpeg.org/download.html)
   - **macOS:** `brew install ffmpeg`
   - **Linux:** `sudo apt install ffmpeg`

2. Optionally set a custom path in `backend/MovieBarcodeReels.Api/appsettings.json`:

```json
"ReelGenerator": {
  "FfmpegPath": "C:\\ffmpeg-8.1.2-essentials_build\\bin\\ffmpeg.exe"
}
```

Leave `FfmpegPath` empty to auto-discover FFmpeg from PATH or common install locations (e.g. `C:\\ffmpeg-*\\bin\\ffmpeg.exe`).

Without FFmpeg, the backend generates a **placeholder file** so you can test uploads, CRUD, and the generation pipeline end-to-end.

## Video Timeline (27 seconds)

| Time | Content |
|------|---------|
| 0–3.5s | "Guess the Movie" + difficulty level |
| 0–27s | Static barcode image (visible until end) |
| 10–15s | Hint: "Released in {Year}" centered above barcode |
| 15–18s | "Leave your answer in the comments" replaces hint |
| 18–22s | "Follow for the reveal" |
| 22–27s | "Follow for movie trivia" |

## Configuration

Backend settings in `appsettings.json`:

- `Storage:UploadsPath` — relative path for uploads (default: `assets/uploads`)
- `Storage:GeneratedPath` — relative path for generated videos (default: `assets/generated`)
- `Storage:DatabasePath` — SQLite database path (default: `Data/moviebarcode.db`)
- `ReelGenerator:FfmpegPath` — optional FFmpeg executable path

Frontend API URL in `frontend/movie-barcode-reels/src/environments/environment.ts`:

```typescript
apiUrl: 'http://localhost:5000'
```

## Architecture Notes

The backend follows a layered architecture ready for future modules:

- **Controllers** — HTTP layer only, no business logic
- **Services** — business logic (movies, uploads, reel generation)
- **Repositories** — data access via EF Core
- **VideoGeneration** — pluggable `IVideoGenerator` with FFmpeg and placeholder implementations

Future modules (movie scraper, AI trivia, batch generation, social uploads, scheduling, analytics) can be added as new services without changing existing endpoints.

## Build for Production

```bash
# Backend
cd MovieBarcodeReels/backend/MovieBarcodeReels.Api
dotnet build -c Release

# Frontend
cd MovieBarcodeReels/frontend/movie-barcode-reels
npm run build
```

## License

MIT
