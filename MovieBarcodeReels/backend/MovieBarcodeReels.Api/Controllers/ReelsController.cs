using Microsoft.AspNetCore.Mvc;
using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Services;

namespace MovieBarcodeReels.Api.Controllers;

[ApiController]
[Route("api/reels")]
public class ReelsController(IReelGeneratorService reelGeneratorService) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<GenerateReelResponse>> Generate(
        [FromBody] GenerateReelRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await reelGeneratorService.GenerateAsync(request.MovieId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (FileNotFoundException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}

[ApiController]
[Route("api/videos")]
public class VideosController(
    IReelGeneratorService reelGeneratorService,
    IFileStorageService fileStorage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GeneratedVideoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var videos = await reelGeneratorService.GetAllVideosAsync(cancellationToken);
        return Ok(videos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GeneratedVideoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var video = await reelGeneratorService.GetVideoByIdAsync(id, cancellationToken);
        return video is null ? NotFound(new ApiErrorResponse($"Video {id} not found.")) : Ok(video);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var video = await reelGeneratorService.GetVideoByIdAsync(id, cancellationToken);
        if (video is null)
        {
            return NotFound(new ApiErrorResponse($"Video {id} not found."));
        }

        var absolutePath = fileStorage.GetAbsolutePath(video.RelativePath);
        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound(new ApiErrorResponse("Video file not found on disk."));
        }

        var contentType = video.UsedPlaceholder ? "application/octet-stream" : "video/mp4";
        var fileName = Path.GetFileName(absolutePath);
        return PhysicalFile(absolutePath, contentType, fileName);
    }
}
