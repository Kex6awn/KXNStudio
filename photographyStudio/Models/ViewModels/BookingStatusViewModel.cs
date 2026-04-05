using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class BookingStatusViewModel
    {
        public int BookingId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
