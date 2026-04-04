using System;
using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = String.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = String.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = String.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
