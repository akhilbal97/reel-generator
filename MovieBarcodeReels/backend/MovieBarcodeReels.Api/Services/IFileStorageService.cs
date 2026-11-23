namespace MovieBarcodeReels.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveUploadAsync(IFormFile file, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
    bool FileExists(string relativePath);
    void EnsureDirectoriesExist();
    string GetGeneratedRelativePath(string fileName);
}
