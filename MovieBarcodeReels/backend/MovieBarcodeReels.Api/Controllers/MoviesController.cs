using Microsoft.AspNetCore.Mvc;
using MovieBarcodeReels.Api.DTOs;
using MovieBarcodeReels.Api.Services;

namespace MovieBarcodeReels.Api.Controllers;

[ApiController]
[Route("api/movies")]
[Route("movies")] // backward compatible
public class MoviesController(IMovieService movieService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MovieDto>>> GetAll(CancellationToken cancellationToken)
    {
        var movies = await movieService.GetAllAsync(cancellationToken);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var movie = await movieService.GetByIdAsync(id, cancellationToken);
        return movie is null ? NotFound(new ApiErrorResponse($"Movie {id} not found.")) : Ok(movie);
    }

    [HttpPost]
    public async Task<ActionResult<MovieDto>> Create(
        [FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ApiErrorResponse("Title is required."));
        }

        var created = await movieService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieDto>> Update(
        int id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ApiErrorResponse("Title is required."));
        }

        var updated = await movieService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound(new ApiErrorResponse($"Movie {id} not found.")) : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await movieService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound(new ApiErrorResponse($"Movie {id} not found."));
    }
}
