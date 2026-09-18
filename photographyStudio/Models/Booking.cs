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

        public SessionWorkflow? SessionWorkflow { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        [RegularExpression(
            @"^[A-Za-z]+(?:[ '-][A-Za-z]+)*$",
            ErrorMessage = "Please enter a valid full name.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Service Type")]
        [Required(ErrorMessage = "Please select a service.")]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Session duration is required.")]
        [Range(1, 12, ErrorMessage = "Session duration must be between 1 and 12 hours.")]
        public int? DurationHours { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = BookingStatuses.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
