using ComplainatorAPI.DTO;

namespace ComplainatorAPI.Services;

public interface IRetrospectiveService
{
    /// <summary>
    /// Creates a new retrospective for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user creating the retrospective.</param>
    /// <returns>The created retrospective data.</returns>
    Task<CreateRetrospectiveResponse> CreateAsync(Guid userId);

    /// <summary>
    /// Gets a paginated list of retrospectives for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose retrospectives to retrieve.</param>
    /// <param name="request">The request parameters for pagination and sorting.</param>
    /// <returns>A paginated list of retrospectives with their accepted suggestions.</returns>
    Task<RetrospectiveListResponse> GetListAsync(Guid userId, RetrospectiveListRequest request);

    /// <summary>
    /// Gets a retrospective by ID for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose retrospective to retrieve.</param>
    /// <param name="retrospectiveId">The ID of the retrospective to retrieve.</param>
    /// <returns>The retrospective details if found and owned by the user, null otherwise.</returns>
    Task<RetrospectiveDetailResponse?> GetByIdAsync(Guid userId, Guid retrospectiveId);

    /// <summary>
    /// Adds a new note to the specified retrospective.
    /// </summary>
    /// <param name="userId">The ID of the user adding the note.</param>
    /// <param name="retrospectiveId">The ID of the retrospective to add the note to.</param>
    /// <param name="request">The note data to add.</param>
    /// <returns>The created note data if successful, null if retrospective not found or not owned by user.</returns>
    Task<CreateNoteResponse?> AddNoteAsync(Guid userId, Guid retrospectiveId, CreateNoteRequest request);
} 