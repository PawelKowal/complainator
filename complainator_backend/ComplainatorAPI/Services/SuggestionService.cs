using ComplainatorAPI.Persistence;
using ComplainatorAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplainatorAPI.Services;

public class SuggestionService : ISuggestionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuggestionService> _logger;

    public SuggestionService(ApplicationDbContext context, ILogger<SuggestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> UpdateStatusAsync(Guid userId, Guid suggestionId, SuggestionStatus status)
    {
        try
        {
            var suggestion = await _context.Suggestions
                .Include(s => s.Retrospective)
                .FirstOrDefaultAsync(s => s.Id == suggestionId);

            if (suggestion == null || suggestion.Retrospective == null || suggestion.Retrospective.UserId != userId)
            {
                _logger.LogWarning("Suggestion {SuggestionId} not found or not owned by user {UserId}", suggestionId, userId);
                return false;
            }

            // If status hasn't changed, return success (idempotency)
            if (suggestion.Status == status)
            {
                return true;
            }

            // Decrement old status counter if necessary
            if (suggestion.Status == SuggestionStatus.Accepted)
            {
                suggestion.Retrospective.AcceptedCount--;
            }
            else if (suggestion.Status == SuggestionStatus.Rejected)
            {
                suggestion.Retrospective.RejectedCount--;
            }

            // Increment new status counter
            if (status == SuggestionStatus.Accepted)
            {
                suggestion.Retrospective.AcceptedCount++;
            }
            else if (status == SuggestionStatus.Rejected)
            {
                suggestion.Retrospective.RejectedCount++;
            }

            var oldStatus = suggestion.Status;
            suggestion.Status = status;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated suggestion {SuggestionId} status from {OldStatus} to {NewStatus}",
                suggestionId, oldStatus, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating suggestion {SuggestionId} status", suggestionId);
            throw;
        }
    }
}