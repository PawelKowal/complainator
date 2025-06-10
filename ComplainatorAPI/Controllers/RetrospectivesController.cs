using ComplainatorAPI.DTO;
using ComplainatorAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplainatorAPI.Controllers;

[ApiController]
[Route("retrospectives")]
[Authorize]
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

    // This action is needed for CreatedAtAction to work
    [HttpGet("{id}")]
    public Task<IActionResult> GetById(Guid id)
    {
        throw new NotImplementedException();
    }
} 