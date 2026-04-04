using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class PhotoUploadViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description {  get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
        public IFormFile? ImageFile { get; set; } = null!;
    }
}
