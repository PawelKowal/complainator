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
    private readonly IAISuggestionService _aiService;

    public RetrospectiveService(
        ApplicationDbContext dbContext,
        ILogger<RetrospectiveService> logger,
        IAISuggestionService aiService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _aiService = aiService;
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
                .Include(r => r.Suggestions)
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
                    Suggestions = r.Suggestions
                        .Where(s => s.Status == SuggestionStatus.Accepted)
                        .Select(s => new SuggestionDto
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

    public async Task<RetrospectiveDetailResponse?> GetByIdAsync(Guid userId, Guid retrospectiveId)
    {
        try
        {
            // Query for the retrospective with its notes and all suggestions
            var retrospective = await _dbContext.Retrospectives
                .AsNoTracking()
                .Include(r => r.Notes)
                .Include(r => r.Suggestions)
                .FirstOrDefaultAsync(r => r.Id == retrospectiveId && r.UserId == userId);

            // Return null if not found or not owned by user
            if (retrospective == null)
            {
                return null;
            }

            // Group notes by category
            var notes = retrospective.Notes.GroupBy(n => n.Category)
                .ToDictionary(g => g.Key, g => g.Select(n => new NoteDto
                {
                    Id = n.Id,
                    Content = n.Content
                }).ToList());

            // Map to response DTO
            return new RetrospectiveDetailResponse
            {
                Id = retrospective.Id,
                Name = retrospective.Name,
                Date = retrospective.Date,
                Notes = new RetrospectiveNotes
                {
                    ImprovementArea = notes.GetValueOrDefault(NoteCategory.ImprovementArea, new List<NoteDto>()),
                    Observation = notes.GetValueOrDefault(NoteCategory.Observation, new List<NoteDto>()),
                    Success = notes.GetValueOrDefault(NoteCategory.Success, new List<NoteDto>())
                },
                Suggestions = retrospective.Suggestions
                    .Select(s => new SuggestionDto
                    {
                        Id = s.Id,
                        SuggestionText = s.SuggestionText,
                        Status = s.Status
                    }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching retrospective {RetrospectiveId} for user {UserId}", retrospectiveId, userId);
            throw;
        }
    }

    public async Task<CreateNoteResponse?> AddNoteAsync(Guid userId, Guid retrospectiveId, CreateNoteRequest request)
    {
        try
        {
            _logger.LogInformation("Adding note to retrospective {RetrospectiveId} for user {UserId}", retrospectiveId, userId);

            // Query for the retrospective to verify ownership
            var retrospective = await _dbContext.Retrospectives
                .FirstOrDefaultAsync(r => r.Id == retrospectiveId && r.UserId == userId);

            // Return null if not found or not owned by user
            if (retrospective == null)
            {
                _logger.LogWarning("Retrospective {RetrospectiveId} not found or not owned by user {UserId}", retrospectiveId, userId);
                return null;
            }

            // Create new note entity
            var note = new Note
            {
                Id = Guid.NewGuid(),
                RetrospectiveId = retrospectiveId,
                Category = request.Category,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            // Add note to database
            await _dbContext.Notes.AddAsync(note);

            // Save changes
            await _dbContext.SaveChangesAsync();

            // Map to response DTO
            return new CreateNoteResponse
            {
                Id = note.Id,
                Category = note.Category,
                Content = note.Content,
                CreatedAt = note.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to retrospective {RetrospectiveId} for user {UserId}", retrospectiveId, userId);
            throw;
        }
    }

    public async Task<GenerateSuggestionsResponse?> GenerateSuggestionsAsync(Guid userId, Guid retrospectiveId)
    {
        try
        {
            _logger.LogInformation("Generating suggestions for retrospective {RetrospectiveId} for user {UserId}", retrospectiveId, userId);

            // Query for the retrospective with its notes and pending suggestions
            var retrospective = await _dbContext.Retrospectives
                .AsNoTracking()
                .Include(r => r.Notes)
                .Include(r => r.Suggestions.Where(s => s.Status == SuggestionStatus.Pending))
                .FirstOrDefaultAsync(r => r.Id == retrospectiveId && r.UserId == userId);

            // Return null if not found or not owned by user
            if (retrospective == null)
            {
                _logger.LogWarning("Retrospective {RetrospectiveId} not found or not owned by user {UserId}", retrospectiveId, userId);
                return null;
            }

            // Check for existing pending suggestions (idempotency)
            if (retrospective.Suggestions.Any())
            {
                _logger.LogInformation("Returning existing pending suggestions for retrospective {RetrospectiveId}", retrospectiveId);
                return new GenerateSuggestionsResponse
                {
                    Suggestions = retrospective.Suggestions.Select(s => new SuggestionDto
                    {
                        Id = s.Id,
                        SuggestionText = s.SuggestionText
                    }).ToList()
                };
            }

            // Generate new suggestions using AI service
            var suggestionTexts = await _aiService.GenerateAsync(retrospective.Notes);

            // Create new suggestion entities
            var suggestions = suggestionTexts.Select(text => new Suggestion
            {
                Id = Guid.NewGuid(),
                RetrospectiveId = retrospectiveId,
                SuggestionText = text,
                Status = SuggestionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // Save new suggestions to database
            await _dbContext.Suggestions.AddRangeAsync(suggestions);
            await _dbContext.SaveChangesAsync();

            // Map to response DTO
            return new GenerateSuggestionsResponse
            {
                Suggestions = suggestions.Select(s => new SuggestionDto
                {
                    Id = s.Id,
                    SuggestionText = s.SuggestionText
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating suggestions for retrospective {RetrospectiveId} for user {UserId}", retrospectiveId, userId);
            throw;
        }
    }
} 