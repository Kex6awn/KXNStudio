using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models
{
    public class ClientNote
    {
        public int ClientNoteId { get; set; }

        [Required]
        public int ClientId { get; set; }

        public Client? Client { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
