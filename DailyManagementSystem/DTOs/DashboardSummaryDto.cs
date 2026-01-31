namespace DailyManagementSystem.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalReceivedAmount { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal Profit { get; set; }
    }
}
