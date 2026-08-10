using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KxnPhotoStudio.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositRequired { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Draft";

        [StringLength(2000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public decimal Total =>
            Math.Max(0, Subtotal + Tax - Discount);

        [NotMapped]
        public decimal BalanceRemaining => Math.Max(0, Total - AmountPaid);

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}