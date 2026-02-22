using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class PhotoUploadViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description {  get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public IFormFile ImageFile { get; set; } = null!;
    }
}
