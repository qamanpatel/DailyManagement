using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyManagementSystem.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int ClientId { get; set; }

        public string? OrderName { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        // --- Work Order Specific Fields ---
        public string? Size { get; set; }
        public string? UOM { get; set; }
        public int Quantity { get; set; } = 1;
        public string? MaterialNo { get; set; }
        public string? CostingLayer { get; set; }
        public string? Color { get; set; }

        // Production Specs
        public string? MaterialSpec { get; set; }
        public string? PaintSpec { get; set; }
        public string? QualitySpec { get; set; }
        public string? WorkNatureSpec { get; set; }
        public string? DurabilitySpec { get; set; }

        // Significant Dates
        public DateTime? ModelingLastDate { get; set; }
        public DateTime? FiberStartDate { get; set; }

        // Sign-offs
        public string? OrderBy { get; set; }
        public string? ModelingBy { get; set; }
        public string? FiberBy { get; set; }

        public string? ImagePath { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("ClientId")]
        public Client? Client { get; set; }
    }
}
