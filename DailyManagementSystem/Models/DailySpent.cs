using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyManagementSystem.Models
{
    [Table("DailySpents")]
    public class DailySpent
    {
        [Key]
        public int SpentId { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public string Category { get; set; } = "General";
        
        [Required]
        public string SpentBy { get; set; } = "Satyanam Patel";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime SpentDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
