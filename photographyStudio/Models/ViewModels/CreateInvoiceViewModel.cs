using System.ComponentModel.DataAnnotations;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class CreateInvoiceViewModel
    {
        public int BookingId { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string ServiceType { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "Tax")]
        public decimal Tax { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "Discount")]
        public decimal Discount { get; set; }

        [Range(0, 999999.99)]
        [Display(Name = "Deposit Required")]
        public decimal DepositRequired { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}