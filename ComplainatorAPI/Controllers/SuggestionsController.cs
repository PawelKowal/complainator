using System.Security.Claims;
using ComplainatorAPI.DTO;
using ComplainatorAPI.Services;
using ComplainatorAPI.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplainatorAPI.Controllers;

[ApiController]
[Route("suggestions")]
[Authorize]
[ModelStateValidation]
public class SuggestionsController : ControllerBase
{
    private readonly ISuggestionService _suggestionService;
    private readonly ILogger<SuggestionsController> _logger;

    public SuggestionsController(ISuggestionService suggestionService, ILogger<SuggestionsController> logger)
    {
        _suggestionService = suggestionService;
        _logger = logger;
    }

    /// <summary>
    /// Updates the status of a suggestion
    /// </summary>
    /// <param name="suggestionId">The ID of the suggestion to update</param>
    /// <param name="request">The new status for the suggestion</param>
    /// <response code="204">Suggestion status was successfully updated</response>
    /// <response code="400">The request was invalid</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="404">Suggestion was not found or does not belong to the user</response>
    [HttpPatch("{suggestionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid suggestionId, [FromBody] UpdateSuggestionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _suggestionService.UpdateStatusAsync(userId, suggestionId, request.Status);

        if (!result)
        {
            return NotFound(new { message = "Suggestion not found" });
        }

        return NoContent();
    }
} 