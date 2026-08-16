using System;
using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        public Invoice? Invoice { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Range(1, 12)]
        public int DurationHours { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = BookingStatuses.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
