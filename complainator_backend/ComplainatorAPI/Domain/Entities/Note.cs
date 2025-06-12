using System;

namespace ComplainatorAPI.Domain.Entities
{
    public class Note
    {
        public Guid Id { get; set; }
        public Guid RetrospectiveId { get; set; }
        public NoteCategory Category { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Retrospective? Retrospective { get; set; }
    }
} 