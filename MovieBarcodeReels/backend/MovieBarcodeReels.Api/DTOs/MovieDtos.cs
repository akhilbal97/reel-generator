namespace MovieBarcodeReels.Api.DTOs;

public record MovieDto(
    int Id,
    string Title,
    int Year,
    string Trivia,
    string Genre,
    string Difficulty,
    string? BarcodeImagePath,
    string? MusicPath,
    DateTime CreatedDate);

public record CreateMovieRequest(
    string Title,
    int Year,
    string Trivia,
    string Genre,
    string Difficulty,
    string? BarcodeImagePath,
    string? MusicPath);

public record UpdateMovieRequest(
    string Title,
    int Year,
    string Trivia,
    string Genre,
    string Difficulty,
    string? BarcodeImagePath,
    string? MusicPath);

public record UploadResponse(string RelativePath, string FileName, long SizeBytes);

public record GenerateReelRequest(int MovieId);

public record GenerateReelResponse(
    int VideoId,
    int MovieId,
    string DownloadUrl,
    string RelativePath,
    long DurationMs,
    bool UsedPlaceholder);

public record GeneratedVideoDto(
    int Id,
    int MovieId,
    string MovieTitle,
    string RelativePath,
    string DownloadUrl,
    DateTime CreatedDate,
    long DurationMs,
    bool UsedPlaceholder);

public record ApiErrorResponse(string Message, string? Detail = null);
