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
} 