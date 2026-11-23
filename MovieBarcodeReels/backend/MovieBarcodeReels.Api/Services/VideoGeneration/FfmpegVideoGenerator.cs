using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using MovieBarcodeReels.Api.Configuration;
using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Services.VideoGeneration;

/// <summary>
/// Generates reels using FFmpeg with the timeline defined in the product spec.
/// </summary>
public class FfmpegVideoGenerator(
    IFfmpegAvailabilityChecker ffmpegChecker,
    IOptions<ReelGeneratorOptions> options,
    ILogger<FfmpegVideoGenerator> logger) : IVideoGenerator
{
    private readonly ReelGeneratorOptions _options = options.Value;

    public async Task<VideoGenerationResult> GenerateAsync(
        Movie movie,
        string barcodeAbsolutePath,
        string outputAbsolutePath,
        CancellationToken cancellationToken = default)
    {
        var ffmpegPath = ffmpegChecker.ResolvedFfmpegPath
            ?? throw new InvalidOperationException("FFmpeg is not available.");

        var workDir = Path.Combine(Path.GetTempPath(), "movie-barcode-reels", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        try
        {
            await WriteTextFilesAsync(workDir, movie, cancellationToken);
            var filterPath = Path.Combine(workDir, "filter.txt");
            var filterGraph = BuildFilterGraph();
            await File.WriteAllTextAsync(filterPath, filterGraph, cancellationToken);

            var localOutput = Path.Combine(workDir, "output.mp4");

            var args = new StringBuilder();
            args.Append($"-y -f lavfi -i color=c=0x111111:s={_options.Width}x{_options.Height}:d={_options.DurationSeconds}:r=30 ");
            args.Append($"-loop 1 -i \"{barcodeAbsolutePath}\" ");
            args.Append("-/filter_complex filter.txt ");
            args.Append("-map \"[vout]\" -c:v libx264 -pix_fmt yuv420p ");
            args.Append($"-t {_options.DurationSeconds} output.mp4");

            logger.LogInformation("Starting FFmpeg generation for movie {MovieId}", movie.Id);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = args.ToString(),
                    WorkingDirectory = workDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0 || !File.Exists(localOutput))
            {
                logger.LogError("FFmpeg failed with exit code {ExitCode}: {Stderr}", process.ExitCode, stderr);
                throw new InvalidOperationException("FFmpeg video generation failed.");
            }

            File.Copy(localOutput, outputAbsolutePath, overwrite: true);

            return new VideoGenerationResult(
                Path.GetFileName(outputAbsolutePath),
                _options.DurationSeconds * 1000L,
                UsedPlaceholder: false);
        }
        finally
        {
            try
            {
                Directory.Delete(workDir, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    private static async Task WriteTextFilesAsync(string workDir, Movie movie, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(Path.Combine(workDir, "intro.txt"), "Guess the Movie", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(workDir, "difficulty.txt"), $"Difficulty: {movie.Difficulty}", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(workDir, "hint.txt"), $"Hint: Released in {movie.Year}", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(workDir, "comments.txt"), "Leave your answer in the comments", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(workDir, "reveal.txt"), "Follow for the reveal", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(workDir, "outro.txt"), "Follow for more movie trivia", cancellationToken);
    }

    private string BuildFilterGraph()
    {
        var w = _options.Width;
        var h = _options.Height;

        // Timeline (27s):
        // 0-3.5s   "Guess the Movie" + difficulty
        // 0-27s    static barcode (visible until end)
        // 10-15s   hint centered on screen (5s)
        // 15-18s   comments replace hint, centered (3s)
        // 18-22s   "Follow for the reveal"
        // 22-27s   "Follow for movie trivia"
        return string.Join(";", new[]
        {
            "[0:v]format=yuv420p,setsar=1[base]",
            $"[1:v]scale={w}:{h}:force_original_aspect_ratio=decrease,pad={w}:{h}:(ow-iw)/2:(oh-ih)/2:color=0x111111[barcode]",
            $"[base][barcode]overlay=0:0:enable='between(t,0,{_options.DurationSeconds})'[v1]",
            DrawText("intro.txt", 72, "(h/2)-55", 0, 3.5, "v1", "v2"),
            DrawText("difficulty.txt", 44, "(h/2)+25", 0, 3.5, "v2", "v3", "0xCCCCCC"),
            DrawText("hint.txt", 48, "(h-text_h)/2", 10, 15, "v3", "v4", "0xCCCCCC"),
            DrawText("comments.txt", 48, "(h-text_h)/2", 15, 18, "v4", "v5"),
            DrawText("reveal.txt", 52, "(h-text_h)/2", 18, 22, "v5", "v6"),
            DrawText("outro.txt", 52, "(h-text_h)/2", 22, 27, "v6", "vout")
        });
    }

    private static string DrawText(
        string textFile,
        int fontSize,
        string y,
        double start,
        double end,
        string input,
        string output,
        string fontColor = "white")
    {
        return $"[{input}]drawtext=font=Arial:textfile={textFile}:fontcolor={fontColor}:fontsize={fontSize}:x=(w-text_w)/2:y={y}:enable='between(t,{start},{end})'[{output}]";
    }
}
