namespace MovieBarcodeReels.Api.Entities;

public class GeneratedVideo
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; }
    public bool UsedPlaceholder { get; set; }
}
