namespace MovieBarcodeReels.Api.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string UploadsPath { get; set; } = "assets/uploads";
    public string GeneratedPath { get; set; } = "assets/generated";
    public string DatabasePath { get; set; } = "Data/moviebarcode.db";
}
