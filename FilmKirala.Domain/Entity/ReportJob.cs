using FilmKirala.Domain.Enums;

namespace FilmKirala.Domain.Entity
{
    public class ReportJob
    {
        public int Id { get; private set; }
        public string JobId { get; private set; } = string.Empty;
        public ReportJobStatus Status { get; private set; }
        public bool IsCsv { get; private set; }
        public string? FilePath { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected ReportJob() { }

        public static ReportJob Create(string jobId, bool isCsv)
            => new ReportJob
            {
                JobId = jobId,
                IsCsv = isCsv,
                Status = ReportJobStatus.Pending,
                CreatedAt = DateTime.Now
            };

        public void MarkAsProcessing() => Status = ReportJobStatus.Processing;

        public void MarkAsCompleted(string filePath)
        {
            Status = ReportJobStatus.Completed;
            FilePath = filePath;
        }

        public void MarkAsFailed() => Status = ReportJobStatus.Failed;
    }
}
