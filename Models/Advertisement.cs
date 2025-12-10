using System.ComponentModel.DataAnnotations;

namespace ogloszenia.Models
{
    public class Advertisement
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Dla HTML w opisach
        public string DetailedDescription { get; set; } = string.Empty;

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        // Kategorie (może być wiele)
        public virtual List<Category> Categories { get; set; } = new();

        // Atrybuty ogłoszenia
        public virtual List<AdvertisementAttributeValue> AttributeValues { get; set; } = new();

        // Multimedia (obrazki, dźwięki, animacje)
        public virtual List<AdvertisementMedia> Media { get; set; } = new();

        // Pliki do pobrania
        public virtual List<AdvertisementFile> Files { get; set; } = new();

        // Status moderacji
        public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Active;

        // Licznik odsłon
        public int ViewCount { get; set; } = 0;

        // Zgłoszenia do moderacji
        public virtual List<ModerationReport> ModerationReports { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum AdvertisementStatus
    {
        Active = 0,
        PendingModeration = 1,
        Rejected = 2,
        Archived = 3
    }

    public class AdvertisementMedia
    {
        public int Id { get; set; }

        public int AdvertisementId { get; set; }
        public virtual Advertisement? Advertisement { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MediaType { get; set; } = "image/jpeg"; // image, audio, video

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }

    public class AdvertisementFile
    {
        public int Id { get; set; }

        public int AdvertisementId { get; set; }
        public virtual Advertisement? Advertisement { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }

    public class ModerationReport
    {
        public int Id { get; set; }

        public int AdvertisementId { get; set; }
        public virtual Advertisement? Advertisement { get; set; }

        public int ReportedByUserId { get; set; }
        public virtual User? ReportedByUser { get; set; }

        public string Reason { get; set; } = string.Empty;

        public ModerationReportStatus Status { get; set; } = ModerationReportStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }
    }

    public enum ModerationReportStatus
    {
        Pending = 0,
        Rejected = 1,
        Approved = 2
    }
}
