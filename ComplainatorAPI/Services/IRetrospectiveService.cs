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
} 