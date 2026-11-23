namespace MovieBarcodeReels.Api.Configuration;

public class ReelGeneratorOptions
{
    public const string SectionName = "ReelGenerator";

    public string? FfmpegPath { get; set; }
    public int Width { get; set; } = 1080;
    public int Height { get; set; } = 1920;
    public int DurationSeconds { get; set; } = 27;
}
