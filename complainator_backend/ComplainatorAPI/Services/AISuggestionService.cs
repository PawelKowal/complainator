using System.Text.Json;
using ComplainatorAPI.Domain.Entities;
using ComplainatorAPI.DTO.OpenRouter;
using Microsoft.Extensions.Logging;

namespace ComplainatorAPI.Services;

public class AISuggestionService : IAISuggestionService
{
    private readonly IOpenRouterService _openRouterService;
    private readonly ILogger<AISuggestionService> _logger;

    public AISuggestionService(
        IOpenRouterService openRouterService,
        ILogger<AISuggestionService> logger)
    {
        _openRouterService = openRouterService;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GenerateAsync(IEnumerable<Note> notes)
    {
        try
        {
            _logger.LogInformation("Generating AI suggestions for {Count} notes", notes.Count());

            // Prepare messages for the AI
            var messages = new List<MessageDto>
            {
                new()
                {
                    Role = "system",
                    Content = @"ROLE: You are a highly efficient agile expert that provides ONLY actionable suggestions.

TASK: Generate 3-5 specific suggestions based on sprint retrospective notes.

STRICT OUTPUT RULES:
1. Start IMMEDIATELY with suggestions
2. Use ONLY bullet points starting with '* '
3. Each suggestion MUST be a single, concrete action
4. NEVER explain your reasoning
5. NEVER add any context or metadata
6. NEVER include anything except the bullet points

CORRECT FORMAT:
* First concrete action
* Second concrete action
* Third concrete action

INCORRECT FORMAT (DO NOT USE):
Here are my suggestions...
1. First suggestion
- Second suggestion
* Third suggestion with explanation because...
Let me explain why...

REMEMBER: Output ONLY the bullet points. Nothing else."
                },
                new()
                {
                    Role = "user",
                    Content = FormatNotesForAI(notes)
                }
            };

            // Call OpenRouter API
            var response = await _openRouterService.SendChatAsync(
                messages,
                parameters: new Dictionary<string, object>
                {
                    { "temperature", 0.7 },
                    { "max_tokens", 2000 }
                });

            // Parse and return suggestions
            if (response is JsonElement jsonElement)
            {
                try 
                {
                    // Check if response contains error
                    if (jsonElement.TryGetProperty("error", out var errorElement))
                    {
                        var errorMessage = errorElement.GetProperty("message").GetString();
                        _logger.LogError("OpenRouter API returned error: {ErrorMessage}", errorMessage);
                        throw new InvalidOperationException($"OpenRouter API error: {errorMessage}");
                    }

                    // Get content from the response
                    var content = jsonElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    if (string.IsNullOrEmpty(content))
                    {
                        throw new InvalidOperationException("Empty response from OpenRouter API");
                    }

                    // Extract suggestions from the formatted text
                    var suggestions = content!
                        .Split('\n')
                        .Where(line => line.Trim().StartsWith("*"))
                        .Select(line => line.TrimStart('*', ' '))
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .ToList();

                    if (!suggestions.Any())
                    {
                        _logger.LogWarning("No suggestions found in the response content");
                        throw new InvalidOperationException("No suggestions found in the response");
                    }

                    _logger.LogInformation("Successfully extracted {Count} AI suggestions", suggestions.Count);
                    foreach (var suggestion in suggestions)
                    {
                        _logger.LogDebug("Extracted suggestion: {Suggestion}", suggestion);
                    }
                    return suggestions;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing OpenRouter API response");
                    throw new InvalidOperationException("Failed to parse suggestions from OpenRouter API response", ex);
                }
            }

            throw new InvalidOperationException("Unexpected response format from OpenRouter API");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestions");
            throw;
        }
    }

    private static string FormatNotesForAI(IEnumerable<Note> notes)
    {
        var categorizedNotes = notes
            .GroupBy(n => n.Category)
            .ToDictionary(g => g.Key, g => g.Select(n => n.Content));

        return $@"Based on these retrospective notes, provide 3-5 actionable suggestions:

What needs improvement:
{FormatCategory(categorizedNotes.GetValueOrDefault(NoteCategory.ImprovementArea))}

Observations:
{FormatCategory(categorizedNotes.GetValueOrDefault(NoteCategory.Observation))}

What went well:
{FormatCategory(categorizedNotes.GetValueOrDefault(NoteCategory.Success))}";
    }

    private static string FormatCategory(IEnumerable<string>? notes)
    {
        if (notes == null || !notes.Any())
        {
            return "- No notes in this category";
        }

        return string.Join("\n", notes.Select(n => $"- {n}"));
    }
} 