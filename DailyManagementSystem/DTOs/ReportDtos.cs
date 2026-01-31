using System;

namespace DailyManagementSystem.DTOs
{
    public class OrderReportDto
    {
        public DateTime Date { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentReportDto
    {
        public DateTime Date { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string OrderName { get; set; } = "N/A";
        public string? BankName { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExpenseReportDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? SpentBy { get; set; }
        public decimal Amount { get; set; }
    }

    public class CategoryExpenseDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class PersonExpenseDto
    {
        public string SpentBy { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
