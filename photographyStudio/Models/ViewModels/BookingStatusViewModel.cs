using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class BookingStatusViewModel
    {
        public int BookingId { get; set; }

        public string CurrentStatus { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public List<SelectListItem> AllowedStatuses { get; set; }
            = new List<SelectListItem>();

        public bool IsFinalStatus { get; set; }
    }
}