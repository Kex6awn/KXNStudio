using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class ClientNotification
    {
        public int ClientNotificationId { get; set; }

        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Subject { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}