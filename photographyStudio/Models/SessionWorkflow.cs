using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class SessionWorkflow
    {
        public int SessionWorkflowId { get; set; }

        [Required]
        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        [StringLength(50)]
        public string EditingStatus { get; set; } = "Not Started";

        public DateTime? EditingStartedAt { get; set; }

        public DateTime? EditingCompletedAt { get; set; }

        [StringLength(50)]
        public string DeliveryStatus { get; set; } = "Not Delivered";

        public DateTime? DeliveredAt { get; set; }

        [StringLength(500)]
        public string? GalleryUrl { get; set; }

        [StringLength(1000)]
        public string? DeliveryNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}