namespace DailyManagementSystem.DTOs
{
    public class ClientSpecificReportDto
    {
        public string ClientName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalPendingAmount { get; set; }
    }
}
