using System.Collections.Generic;
using System.Threading.Tasks;
using DailyManagementSystem.DTOs;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<IEnumerable<ClientMonthlyReportDto>> GetMonthlyClientReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<IEnumerable<OrderReportDto>> GetMonthlyOrderReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<IEnumerable<PaymentReportDto>> GetMonthlyPaymentReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<IEnumerable<ExpenseReportDto>> GetMonthlyExpenseReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<IEnumerable<CategoryExpenseDto>> GetMonthlyCategoryExpenseSummaryAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<IEnumerable<PersonExpenseDto>> GetMonthlyPersonExpenseSummaryAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
        Task<ClientSpecificReportDto> GetClientSpecificReportAsync(int clientId, int? startYear, int? startMonth, int? endYear = null, int? endMonth = null);
    }
}
