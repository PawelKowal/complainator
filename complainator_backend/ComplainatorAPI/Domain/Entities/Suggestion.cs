using System;

namespace ComplainatorAPI.Domain.Entities
{
    public class Suggestion
    {
        public Guid Id { get; set; }
        public Guid RetrospectiveId { get; set; }
        public required string SuggestionText { get; set; }
        public SuggestionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Retrospective? Retrospective { get; set; }
    }
}