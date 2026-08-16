using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class SessionWorkflowViewModel
    {
        public int BookingId { get; set; }

        public int SessionWorkflowId { get; set; }

        [Required]
        public string EditingStatus { get; set; } = "Not Started";

        [Required]
        public string DeliveryStatus { get; set; } = "Not Delivered";

        [StringLength(500)]
        [Display(Name = "Gallery URL")]
        public string? GalleryUrl { get; set; }

        [StringLength(1000)]
        [Display(Name = "Delivery Notes")]
        public string? DeliveryNotes { get; set; }
    }
}