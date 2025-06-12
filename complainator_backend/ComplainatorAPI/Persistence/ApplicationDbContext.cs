using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Retrospective> Retrospectives { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Retrospective entity
            builder.Entity<Retrospective>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.AcceptedCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.RejectedCount)
                    .HasDefaultValue(0);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create index on UserId
                entity.HasIndex(e => e.UserId);
            });

            // Configure Note entity
            builder.Entity<Note>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Convert enum to string
                entity.Property(e => e.Category)
                    .HasConversion<string>();

                entity.HasOne(e => e.Retrospective)
                    .WithMany(r => r.Notes)
                    .HasForeignKey(e => e.RetrospectiveId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create index on RetrospectiveId
                entity.HasIndex(e => e.RetrospectiveId);
            });

            // Configure Suggestion entity
            builder.Entity<Suggestion>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Convert enum to string
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.Retrospective)
                    .WithMany(r => r.Suggestions)
                    .HasForeignKey(e => e.RetrospectiveId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create index on RetrospectiveId
                entity.HasIndex(e => e.RetrospectiveId);
            });

            // Configure AuditLog entity
            builder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Convert enum to string
                entity.Property(e => e.Level)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Retrospective)
                    .WithMany(r => r.AuditLogs)
                    .HasForeignKey(e => e.RetrospectiveId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Create indices on UserId and RetrospectiveId
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.RetrospectiveId);
            });
        }
    }
}