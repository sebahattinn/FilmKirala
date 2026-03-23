using FilmKirala.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilmKirala.Infrastructure.Persistence.Configurations
{
    public class ReportJobConfiguration : IEntityTypeConfiguration<ReportJob>
    {
        public void Configure(EntityTypeBuilder<ReportJob> builder)
        {
            // Composite index: background worker polls WHERE Status = 0 ORDER BY CreatedAt
            // Without this, every poll does a full table scan (was taking 25 seconds).
            builder.HasIndex(r => new { r.Status, r.CreatedAt })
                   .HasDatabaseName("IX_ReportJobs_Status_CreatedAt");

            builder.Property(r => r.JobId).HasMaxLength(50);
        }
    }
}
