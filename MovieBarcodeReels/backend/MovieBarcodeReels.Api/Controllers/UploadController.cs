using Microsoft.AspNetCore.Mvc;
using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Services;

namespace MovieBarcodeReels.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController(
    IFileStorageService fileStorage,
    ILogger<UploadController> logger) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("No file uploaded."));
        }

        try
        {
            var relativePath = await fileStorage.SaveUploadAsync(file, cancellationToken);
            return Ok(new UploadResponse(relativePath, file.FileName, file.Length));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid upload rejected");
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}
