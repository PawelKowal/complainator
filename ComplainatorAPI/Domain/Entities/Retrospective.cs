using System;
using System.Collections.Generic;

namespace ComplainatorAPI.Domain.Entities
{
    public class Retrospective
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
} 