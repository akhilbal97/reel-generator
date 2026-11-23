using Microsoft.EntityFrameworkCore;
using MovieBarcodeReels.Api.Entities;

namespace MovieBarcodeReels.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<GeneratedVideo> GeneratedVideos => Set<GeneratedVideo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(m => m.Title).HasMaxLength(256).IsRequired();
            entity.Property(m => m.Genre).HasMaxLength(128);
            entity.Property(m => m.Difficulty).HasMaxLength(64);
            entity.Property(m => m.Trivia).HasMaxLength(2000);
            entity.Property(m => m.BarcodeImagePath).HasMaxLength(512);
            entity.Property(m => m.MusicPath).HasMaxLength(512);
        });

        modelBuilder.Entity<GeneratedVideo>(entity =>
        {
            entity.Property(v => v.FilePath).HasMaxLength(512).IsRequired();
            entity.HasOne(v => v.Movie)
                .WithMany(m => m.GeneratedVideos)
                .HasForeignKey(v => v.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
