using System;

namespace DailyManagementSystem.DTOs
{
    public class ExpenseDto
    {
        public int SpentId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string SpentBy { get; set; } = "Satyanam Patel";
        public decimal Amount { get; set; }
        public DateTime SpentDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
