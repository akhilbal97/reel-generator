using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Repositories;

public interface IGeneratedVideoRepository
{
    Task<IReadOnlyList<GeneratedVideo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GeneratedVideo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GeneratedVideo> AddAsync(GeneratedVideo video, CancellationToken cancellationToken = default);
    Task DeleteAsync(GeneratedVideo video, CancellationToken cancellationToken = default);
}
