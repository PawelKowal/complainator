using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.Services;

public interface IAISuggestionService
{
    /// <summary>
    /// Generates AI suggestions based on the provided notes.
    /// </summary>
    /// <param name="notes">The collection of notes to analyze.</param>
    /// <returns>A list of suggestion texts.</returns>
    Task<IEnumerable<string>> GenerateAsync(IEnumerable<Note> notes);
} 