using System.Text;
using Microsoft.Extensions.Options;
using MovieBarcodeReels.Api.Configuration;
using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Services.VideoGeneration;

/// <summary>
/// Fallback generator when FFmpeg is unavailable. Writes a minimal valid MP4 stub
/// so the pipeline can be tested end-to-end. Replace with real rendering when FFmpeg is installed.
/// </summary>
public class PlaceholderVideoGenerator(
    IOptions<ReelGeneratorOptions> options,
    ILogger<PlaceholderVideoGenerator> logger) : IVideoGenerator
{
    private readonly ReelGeneratorOptions _options = options.Value;

    public Task<VideoGenerationResult> GenerateAsync(
        Movie movie,
        string barcodeAbsolutePath,
        string outputAbsolutePath,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Generating placeholder video for movie {MovieId} ({Title}). Install FFmpeg for full reel output.",
            movie.Id, movie.Title);

        // Minimal fMP4-like stub bytes — enough for download/testing; not a playable reel.
        // In production, FFmpeg replaces this entirely.
        var placeholderContent = Encoding.UTF8.GetBytes(
            $"PLACEHOLDER_REEL|movie={movie.Id}|title={movie.Title}|duration={_options.DurationSeconds}s");

        File.WriteAllBytes(outputAbsolutePath, placeholderContent);

        return Task.FromResult(new VideoGenerationResult(
            Path.GetFileName(outputAbsolutePath),
            _options.DurationSeconds * 1000L,
            UsedPlaceholder: true));
    }
}
