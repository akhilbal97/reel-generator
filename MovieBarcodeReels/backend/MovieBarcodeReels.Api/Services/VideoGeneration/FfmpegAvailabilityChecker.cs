using System.Diagnostics;
using Microsoft.Extensions.Options;
using MovieBarcodeReels.Api.Configuration;

namespace MovieBarcodeReels.Api.Services.VideoGeneration;

public class FfmpegAvailabilityChecker : IFfmpegAvailabilityChecker
{
    private readonly ReelGeneratorOptions _options;
    private readonly ILogger<FfmpegAvailabilityChecker> _logger;
    private string? _resolvedPath;
    private bool _checked;

    public FfmpegAvailabilityChecker(
        IOptions<ReelGeneratorOptions> options,
        ILogger<FfmpegAvailabilityChecker> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string? ResolvedFfmpegPath => _resolvedPath;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (_checked)
        {
            return _resolvedPath is not null;
        }

        _checked = true;

        foreach (var candidate in GetCandidatePaths())
        {
            if (await TryFfmpegAsync(candidate, cancellationToken))
            {
                _resolvedPath = candidate;
                _logger.LogInformation("FFmpeg found at {FfmpegPath}", candidate);
                return true;
            }
        }

        _logger.LogWarning("FFmpeg not found. Placeholder video generation will be used.");
        return false;
    }

    private IEnumerable<string> GetCandidatePaths()
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(_options.FfmpegPath))
        {
            candidates.Add(_options.FfmpegPath);
        }

        candidates.Add("ffmpeg");

        if (OperatingSystem.IsWindows())
        {
            candidates.Add(@"C:\ffmpeg\bin\ffmpeg.exe");
            candidates.AddRange(DiscoverWindowsInstallPaths());
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> DiscoverWindowsInstallPaths()
    {
        var paths = new List<string>();

        try
        {
            foreach (var dir in Directory.EnumerateDirectories(@"C:\", "ffmpeg*"))
            {
                var exe = Path.Combine(dir, "bin", "ffmpeg.exe");
                if (File.Exists(exe))
                {
                    paths.Add(exe);
                }
            }
        }
        catch
        {
            // Best effort discovery
        }

        var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (Directory.Exists(downloads))
        {
            try
            {
                foreach (var exe in Directory.EnumerateFiles(downloads, "ffmpeg.exe", SearchOption.AllDirectories))
                {
                    paths.Add(exe);
                }
            }
            catch
            {
                // Best effort discovery
            }
        }

        return paths;
    }

    private async Task<bool> TryFfmpegAsync(string path, CancellationToken cancellationToken)
    {
        if (path != "ffmpeg" && !File.Exists(path))
        {
            _logger.LogDebug("FFmpeg not found at {Path}", path);
            return false;
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "FFmpeg check failed for {Path}", path);
            return false;
        }
    }
}
