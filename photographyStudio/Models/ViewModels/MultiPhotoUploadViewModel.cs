using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class MultiPhotoUploadViewModel
    {
        [Required]
        public int CategoryId { get; set; }

        public string? Description { get; set; }

        [Required]
        public List<IFormFile> ImageFiles { get; set; } = new();
    }
}
