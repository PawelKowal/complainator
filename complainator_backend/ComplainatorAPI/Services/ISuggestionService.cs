using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.Services;

public interface ISuggestionService
{
    /// <summary>
    /// Updates the status of a suggestion and related retrospective counters
    /// </summary>
    /// <param name="userId">The ID of the user making the request</param>
    /// <param name="suggestionId">The ID of the suggestion to update</param>
    /// <param name="status">The new status to set</param>
    /// <returns>True if suggestion was found and updated, false if not found or not owned by user</returns>
    Task<bool> UpdateStatusAsync(Guid userId, Guid suggestionId, SuggestionStatus status);
}