using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Services.VideoGeneration;

public record VideoGenerationResult(
    string OutputRelativePath,
    long DurationMs,
    bool UsedPlaceholder);

public interface IVideoGenerator
{
    Task<VideoGenerationResult> GenerateAsync(
        Movie movie,
        string barcodeAbsolutePath,
        string outputAbsolutePath,
        CancellationToken cancellationToken = default);
}

public interface IFfmpegAvailabilityChecker
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    string? ResolvedFfmpegPath { get; }
}
