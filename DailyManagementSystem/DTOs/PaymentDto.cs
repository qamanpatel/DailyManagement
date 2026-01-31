using System;

namespace DailyManagementSystem.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int ClientId { get; set; }
        public int? OrderId { get; set; }
        public decimal AmountReceived { get; set; }
        public string? BankName { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Extended Properties
        public string ClientName { get; set; } = string.Empty;
        public string OrderName { get; set; } = "N/A"; // "Advance" or OrderName
    }
}
