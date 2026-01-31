using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyManagementSystem.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ClientId { get; set; }

        public int? OrderId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountReceived { get; set; }

        public string? BankName { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
