using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class RecordPaymentViewModel
    {
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal BalanceRemaining { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string Method { get; set; } = "Cash";

        [StringLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}