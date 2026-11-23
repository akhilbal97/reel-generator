using Microsoft.EntityFrameworkCore;
using MovieBarcodeReels.Api.Data;
using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Repositories;

public class GeneratedVideoRepository(AppDbContext dbContext) : IGeneratedVideoRepository
{
    public async Task<IReadOnlyList<GeneratedVideo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.GeneratedVideos
            .Include(v => v.Movie)
            .OrderByDescending(v => v.CreatedDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<GeneratedVideo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.GeneratedVideos
            .Include(v => v.Movie)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<GeneratedVideo> AddAsync(GeneratedVideo video, CancellationToken cancellationToken = default)
    {
        dbContext.GeneratedVideos.Add(video);
        await dbContext.SaveChangesAsync(cancellationToken);
        return video;
    }

    public async Task DeleteAsync(GeneratedVideo video, CancellationToken cancellationToken = default)
    {
        dbContext.GeneratedVideos.Remove(video);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
