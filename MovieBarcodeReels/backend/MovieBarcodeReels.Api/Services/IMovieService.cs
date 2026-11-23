using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Services;

public interface IMovieService
{
    Task<IReadOnlyList<MovieDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MovieDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MovieDto> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default);
    Task<MovieDto?> UpdateAsync(int id, UpdateMovieRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public static class MovieMapper
{
    public static MovieDto ToDto(Movie movie) => new(
        movie.Id,
        movie.Title,
        movie.Year,
        movie.Trivia,
        movie.Genre,
        movie.Difficulty,
        movie.BarcodeImagePath,
        movie.MusicPath,
        movie.CreatedDate);
}
