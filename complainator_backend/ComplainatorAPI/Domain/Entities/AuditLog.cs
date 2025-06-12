using System;
using Microsoft.Extensions.Logging;

namespace ComplainatorAPI.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? UserId { get; set; }
        public LogLevel Level { get; set; }
        public required string Message { get; set; }
        public Guid? RetrospectiveId { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Retrospective? Retrospective { get; set; }
    }
}