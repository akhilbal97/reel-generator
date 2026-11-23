using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Entities;
using MovieBarcodeReels.Api.Repositories;
using MovieBarcodeReels.Api.Services.VideoGeneration;

namespace MovieBarcodeReels.Api.Services;

public interface IReelGeneratorService
{
    Task<GenerateReelResponse> GenerateAsync(int movieId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneratedVideoDto>> GetAllVideosAsync(CancellationToken cancellationToken = default);
    Task<GeneratedVideoDto?> GetVideoByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class ReelGeneratorService(
    IMovieRepository movieRepository,
    IGeneratedVideoRepository videoRepository,
    IFileStorageService fileStorage,
    IFfmpegAvailabilityChecker ffmpegChecker,
    FfmpegVideoGenerator ffmpegGenerator,
    PlaceholderVideoGenerator placeholderGenerator,
    ILogger<ReelGeneratorService> logger) : IReelGeneratorService
{
    public async Task<GenerateReelResponse> GenerateAsync(int movieId, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var movie = await movieRepository.GetByIdAsync(movieId, cancellationToken)
            ?? throw new KeyNotFoundException($"Movie with id {movieId} was not found.");

        if (string.IsNullOrWhiteSpace(movie.BarcodeImagePath))
        {
            throw new InvalidOperationException("Movie does not have a barcode image.");
        }

        if (!fileStorage.FileExists(movie.BarcodeImagePath))
        {
            throw new FileNotFoundException("Barcode image file was not found on disk.", movie.BarcodeImagePath);
        }

        var fileName = $"reel_{movieId}_{DateTime.UtcNow:yyyyMMddHHmmss}.mp4";
        var relativePath = fileStorage.GetGeneratedRelativePath(fileName);
        var outputAbsolutePath = fileStorage.GetAbsolutePath(relativePath);
        var barcodeAbsolutePath = fileStorage.GetAbsolutePath(movie.BarcodeImagePath);

        logger.LogInformation("Starting reel generation for movie {MovieId}", movieId);

        VideoGenerationResult result;
        try
        {
            var ffmpegAvailable = await ffmpegChecker.IsAvailableAsync(cancellationToken);
            var generator = ffmpegAvailable ? (IVideoGenerator)ffmpegGenerator : placeholderGenerator;
            result = await generator.GenerateAsync(movie, barcodeAbsolutePath, outputAbsolutePath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Video generation failed for movie {MovieId}, falling back to placeholder", movieId);
            result = await placeholderGenerator.GenerateAsync(movie, barcodeAbsolutePath, outputAbsolutePath, cancellationToken);
        }

        var video = new GeneratedVideo
        {
            MovieId = movieId,
            FilePath = relativePath,
            CreatedDate = DateTime.UtcNow,
            DurationMs = result.DurationMs,
            UsedPlaceholder = result.UsedPlaceholder
        };

        await videoRepository.AddAsync(video, cancellationToken);

        stopwatch.Stop();
        logger.LogInformation(
            "Reel generation completed for movie {MovieId} in {ElapsedMs}ms (placeholder={UsedPlaceholder})",
            movieId, stopwatch.ElapsedMilliseconds, result.UsedPlaceholder);

        return new GenerateReelResponse(
            video.Id,
            movieId,
            $"/api/videos/{video.Id}/download",
            relativePath,
            result.DurationMs,
            result.UsedPlaceholder);
    }

    public async Task<IReadOnlyList<GeneratedVideoDto>> GetAllVideosAsync(CancellationToken cancellationToken = default)
    {
        var videos = await videoRepository.GetAllAsync(cancellationToken);
        return videos.Select(ToDto).ToList();
    }

    public async Task<GeneratedVideoDto?> GetVideoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var video = await videoRepository.GetByIdAsync(id, cancellationToken);
        return video is null ? null : ToDto(video);
    }

    private static GeneratedVideoDto ToDto(GeneratedVideo video) => new(
        video.Id,
        video.MovieId,
        video.Movie?.Title ?? string.Empty,
        video.FilePath,
        $"/api/videos/{video.Id}/download",
        video.CreatedDate,
        video.DurationMs,
        video.UsedPlaceholder);
}
