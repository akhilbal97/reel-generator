using Microsoft.EntityFrameworkCore;
using MovieBarcodeReels.Api.Data;
using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Repositories;

public class MovieRepository(AppDbContext dbContext) : IMovieRepository
{
    public async Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Movies
            .OrderByDescending(m => m.CreatedDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Movies
            .Include(m => m.GeneratedVideos)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
        return movie;
    }

    public async Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        dbContext.Movies.Update(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
