using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Entities;
using MovieBarcodeReels.Api.Repositories;

namespace MovieBarcodeReels.Api.Services;

public class MovieService(
    IMovieRepository movieRepository,
    ILogger<MovieService> logger) : IMovieService
{
    public async Task<IReadOnlyList<MovieDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await movieRepository.GetAllAsync(cancellationToken);
        return movies.Select(MovieMapper.ToDto).ToList();
    }

    public async Task<MovieDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken);
        return movie is null ? null : MovieMapper.ToDto(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var movie = new Movie
        {
            Title = request.Title.Trim(),
            Year = request.Year,
            Trivia = request.Trivia.Trim(),
            Genre = request.Genre.Trim(),
            Difficulty = request.Difficulty.Trim(),
            BarcodeImagePath = request.BarcodeImagePath,
            MusicPath = request.MusicPath,
            CreatedDate = DateTime.UtcNow
        };

        var created = await movieRepository.AddAsync(movie, cancellationToken);
        logger.LogInformation("Created movie {MovieId}: {Title}", created.Id, created.Title);
        return MovieMapper.ToDto(created);
    }

    public async Task<MovieDto?> UpdateAsync(int id, UpdateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return null;
        }

        movie.Title = request.Title.Trim();
        movie.Year = request.Year;
        movie.Trivia = request.Trivia.Trim();
        movie.Genre = request.Genre.Trim();
        movie.Difficulty = request.Difficulty.Trim();
        movie.BarcodeImagePath = request.BarcodeImagePath;
        movie.MusicPath = request.MusicPath;

        await movieRepository.UpdateAsync(movie, cancellationToken);
        logger.LogInformation("Updated movie {MovieId}: {Title}", movie.Id, movie.Title);
        return MovieMapper.ToDto(movie);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return false;
        }

        await movieRepository.DeleteAsync(movie, cancellationToken);
        logger.LogInformation("Deleted movie {MovieId}: {Title}", movie.Id, movie.Title);
        return true;
    }
}
