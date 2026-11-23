using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MovieBarcodeReels.Api.Configuration;
using MovieBarcodeReels.Api.Data;
using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Repositories;
using MovieBarcodeReels.Api.Services;
using MovieBarcodeReels.Api.Services.VideoGeneration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.Configure<ReelGeneratorOptions>(builder.Configuration.GetSection(ReelGeneratorOptions.SectionName));

var storageOptions = builder.Configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>()
    ?? new StorageOptions();

var contentRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));
var dbPath = Path.Combine(contentRoot, storageOptions.DatabasePath.Replace('/', Path.DirectorySeparatorChar));
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Movie Barcode Reels API",
        Version = "v1",
        Description = "API for managing movies, uploads, and reel generation."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IGeneratedVideoRepository, GeneratedVideoRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IReelGeneratorService, ReelGeneratorService>();
builder.Services.AddSingleton<IFfmpegAvailabilityChecker, FfmpegAvailabilityChecker>();
builder.Services.AddScoped<FfmpegVideoGenerator>();
builder.Services.AddScoped<PlaceholderVideoGenerator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
    fileStorage.EnsureDirectoriesExist();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Barcode Reels API v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" })).ExcludeFromDescription();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
            "An unexpected error occurred.",
            app.Environment.IsDevelopment() ? exception?.Message : null));
    });
});

app.UseCors("Frontend");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(contentRoot, storageOptions.UploadsPath)),
    RequestPath = "/assets/uploads"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(contentRoot, storageOptions.GeneratedPath)),
    RequestPath = "/generated"
});

app.MapControllers();

app.Run();
