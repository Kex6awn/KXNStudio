using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KxnPhotoStudio.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(50)]
        public string Method { get; set; } = "Manual";

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}