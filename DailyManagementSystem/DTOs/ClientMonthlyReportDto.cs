namespace DailyManagementSystem.DTOs
{
    public class ClientMonthlyReportDto
    {
        public string ClientName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
    }
}
