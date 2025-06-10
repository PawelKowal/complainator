using ComplainatorAPI.Domain.Entities;
using ComplainatorAPI.DTO;
using ComplainatorAPI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplainatorAPI.Services;

public class RetrospectiveService : IRetrospectiveService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RetrospectiveService> _logger;

    public RetrospectiveService(ApplicationDbContext dbContext, ILogger<RetrospectiveService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateRetrospectiveResponse> CreateAsync(Guid userId)
    {
        try
        {
            // Get count of existing retrospectives for the user
            var count = await _dbContext.Retrospectives
                .CountAsync(r => r.UserId == userId);

            // Generate name in format "Retrospektywa #{count+1} - DD.MM.YYYY"
            var name = $"Retrospektywa #{count + 1} - {DateTime.UtcNow:dd.MM.yyyy}";

            // Create new retrospective entity
            var retrospective = new Retrospective
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Date = DateTime.UtcNow.Date
            };

            // Save to database
            await _dbContext.Retrospectives.AddAsync(retrospective);
            await _dbContext.SaveChangesAsync();

            // Map to response DTO
            return new CreateRetrospectiveResponse
            {
                Id = retrospective.Id,
                Name = retrospective.Name,
                Date = retrospective.Date
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating retrospective for user {UserId}", userId);
            throw;
        }
    }

    public async Task<RetrospectiveListResponse> GetListAsync(Guid userId, RetrospectiveListRequest request)
    {
        try
        {
            // Create base query for user's retrospectives
            var query = _dbContext.Retrospectives
                .Include(r => r.Suggestions.Where(s => s.Status == SuggestionStatus.Accepted))
                .Where(r => r.UserId == userId)
                .AsNoTracking();

            // Apply sorting
            query = request.Sort switch
            {
                SortOrder.DateDesc => query.OrderByDescending(r => r.Date),
                SortOrder.DateAsc => query.OrderBy(r => r.Date),
                _ => query.OrderByDescending(r => r.Date) // Default to DateDesc
            };

            // Get total count
            var total = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((request.Page - 1) * request.PerPage)
                .Take(request.PerPage)
                .Select(r => new RetrospectiveListItem
                {
                    Id = r.Id,
                    Name = r.Name,
                    Date = r.Date,
                    AcceptedSuggestions = r.Suggestions.Select(s => new SuggestionListItem
                    {
                        Id = s.Id,
                        SuggestionText = s.SuggestionText
                    }).ToList()
                })
                .ToListAsync();

            // Return response
            return new RetrospectiveListResponse
            {
                Items = items,
                Total = total,
                Page = request.Page,
                PerPage = request.PerPage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching retrospectives for user {UserId}", userId);
            throw;
        }
    }
} 