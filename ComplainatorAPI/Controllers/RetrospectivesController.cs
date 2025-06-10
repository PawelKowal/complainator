using ComplainatorAPI.DTO;
using ComplainatorAPI.Services;
using ComplainatorAPI.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplainatorAPI.Controllers;

[ApiController]
[Route("retrospectives")]
[Authorize]
[ModelStateValidation]
public class RetrospectivesController : ControllerBase
{
    private readonly IRetrospectiveService _retrospectiveService;
    private readonly ILogger<RetrospectivesController> _logger;

    public RetrospectivesController(
        IRetrospectiveService retrospectiveService,
        ILogger<RetrospectivesController> logger)
    {
        _retrospectiveService = retrospectiveService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new retrospective for the authenticated user.
    /// </summary>
    /// <returns>The created retrospective data.</returns>
    /// <response code="201">Returns the newly created retrospective</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRetrospectiveResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim not found in token");
            return Unauthorized();
        }

        var response = await _retrospectiveService.CreateAsync(Guid.Parse(userId));
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Gets a retrospective by ID for the authenticated user.
    /// </summary>
    /// <param name="id">The ID of the retrospective to retrieve.</param>
    /// <returns>The retrospective details if found and owned by the user.</returns>
    /// <response code="200">Returns the retrospective details</response>
    /// <response code="400">If the ID is not a valid GUID</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the retrospective is not found or not owned by the user</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RetrospectiveDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim not found in token");
            return Unauthorized();
        }

        try
        {
            var response = await _retrospectiveService.GetByIdAsync(Guid.Parse(userId), id);
            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching retrospective {RetrospectiveId} for user {UserId}", id, userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching the retrospective" });
        }
    }

    /// <summary>
    /// Gets a paginated list of retrospectives for the authenticated user.
    /// </summary>
    /// <param name="request">The request parameters for pagination and sorting.</param>
    /// <returns>A paginated list of retrospectives with their accepted suggestions.</returns>
    /// <response code="200">Returns the list of retrospectives</response>
    /// <response code="400">If the request parameters are invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(RetrospectiveListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList([FromQuery] RetrospectiveListRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim not found in token");
            return Unauthorized();
        }

        try
        {
            var response = await _retrospectiveService.GetListAsync(Guid.Parse(userId), request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching retrospectives for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while fetching retrospectives" });
        }
    }

    /// <summary>
    /// Adds a new note to a retrospective.
    /// </summary>
    /// <param name="id">The ID of the retrospective to add the note to.</param>
    /// <param name="request">The note data to add.</param>
    /// <returns>The created note data.</returns>
    /// <response code="201">Returns the newly created note</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the retrospective is not found or not owned by the user</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("{id}/notes")]
    [ProducesResponseType(typeof(CreateNoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] CreateNoteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID claim not found in token");
            return Unauthorized();
        }

        try
        {
            var response = await _retrospectiveService.AddNoteAsync(Guid.Parse(userId), id, request);
            if (response == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetById), new { id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to retrospective {RetrospectiveId} for user {UserId}", id, userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while adding the note" });
        }
    }
} 