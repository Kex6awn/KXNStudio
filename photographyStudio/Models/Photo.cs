using System;
using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // FK + Navigation
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}