namespace MovieBarcodeReels.Api.Entities;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Trivia { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string? BarcodeImagePath { get; set; }
    public string? MusicPath { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<GeneratedVideo> GeneratedVideos { get; set; } = [];
}
