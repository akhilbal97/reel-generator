using Microsoft.Extensions.Options;
using MovieBarcodeReels.Api.Configuration;

namespace MovieBarcodeReels.Api.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpeg", ".jpg"
    };

    private static readonly HashSet<string> AllowedAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3"
    };

    private readonly StorageOptions _options;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _contentRoot;

    public FileStorageService(
        IOptions<StorageOptions> options,
        IWebHostEnvironment environment,
        ILogger<FileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _contentRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", ".."));
    }

    public void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(GetAbsolutePath(_options.UploadsPath));
        Directory.CreateDirectory(GetAbsolutePath(_options.GeneratedPath));
        Directory.CreateDirectory(Path.GetDirectoryName(GetAbsolutePath(_options.DatabasePath))!);
    }

    public async Task<string> SaveUploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension) && !AllowedAudioExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Invalid file type. Allowed: PNG, JPEG, JPG, MP3.");
        }

        var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var relativePath = Path.Combine(_options.UploadsPath, safeFileName).Replace('\\', '/');
        var absolutePath = GetAbsolutePath(relativePath);

        await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
        await file.CopyToAsync(stream, cancellationToken);

        _logger.LogInformation("Saved upload {FileName} ({SizeBytes} bytes) to {RelativePath}",
            file.FileName, file.Length, relativePath);

        return relativePath;
    }

    public string GetAbsolutePath(string relativePath)
    {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(_contentRoot, normalized));
    }

    public bool FileExists(string relativePath)
    {
        return File.Exists(GetAbsolutePath(relativePath));
    }

    public string GetGeneratedRelativePath(string fileName)
    {
        return Path.Combine(_options.GeneratedPath, fileName).Replace('\\', '/');
    }
}
