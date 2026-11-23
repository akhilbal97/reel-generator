using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task DeleteAsync(Movie movie, CancellationToken cancellationToken = default);
}
