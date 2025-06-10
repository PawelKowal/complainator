using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.Services;

public class MockAISuggestionService : IAISuggestionService
{
    private readonly ILogger<MockAISuggestionService> _logger;

    public MockAISuggestionService(ILogger<MockAISuggestionService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GenerateAsync(IEnumerable<Note> notes)
    {
        try
        {
            _logger.LogInformation("Generating mock AI suggestions for {Count} notes", notes.Count());

            // Simulate API delay
            await Task.Delay(1000);

            // Return mock suggestions
            return new[]
            {
                "Consider implementing regular team knowledge sharing sessions to address observed communication gaps.",
                "Based on success patterns, establish a formal process for documenting and sharing best practices.",
                "To improve team velocity, introduce automated testing for frequently reported problem areas.",
                "Schedule dedicated time for technical debt reduction based on recurring improvement suggestions.",
                "Implement pair programming sessions for complex tasks to enhance code quality and knowledge transfer."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestions");
            throw;
        }
    }
} 