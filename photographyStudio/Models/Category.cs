using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public List<Photo> Photos { get; set; } = new();
    }
}
