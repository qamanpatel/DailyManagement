using System.Collections.Generic;
using System.Threading.Tasks;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Services.Interfaces
{
    public class ReportData
    {
        public int? StartYear { get; set; }
        public int? StartMonth { get; set; }
        public int? EndYear { get; set; }
        public int? EndMonth { get; set; }
        public DashboardSummaryDto Summary { get; set; } = new();
        public IEnumerable<OrderReportDto> Orders { get; set; } = new List<OrderReportDto>();
        public IEnumerable<PaymentReportDto> Payments { get; set; } = new List<PaymentReportDto>();
        public IEnumerable<ExpenseReportDto> Expenses { get; set; } = new List<ExpenseReportDto>();
        public IEnumerable<CategoryExpenseDto> CategorySummary { get; set; } = new List<CategoryExpenseDto>();
    }

    public interface IExportService
    {
        Task ExportReportToPdfAsync(string filePath, ReportData data);
        Task ExportWorkOrderToPdfAsync(string filePath, Order order);
    }
}
