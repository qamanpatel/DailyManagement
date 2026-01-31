using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DailyManagementSystem.Data;
using DailyManagementSystem.Models;
using DailyManagementSystem.DTOs;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        private (DateTime? start, DateTime? end) GetDateRange(int? startYear, int? startMonth, int? endYear, int? endMonth)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (startYear.HasValue)
            {
                startDate = new DateTime(startYear.Value, startMonth ?? 1, 1);

                if (endYear.HasValue)
                {
                    endDate = new DateTime(endYear.Value, endMonth ?? 12, 1).AddMonths(1).AddDays(-1);
                }
                else
                {
                    if (startMonth.HasValue)
                    {
                        // Single month if only start is provided
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                    }
                    else
                    {
                        // Full year if month is null
                        endDate = new DateTime(startYear.Value, 12, 31);
                    }
                }
            }

            return (startDate, endDate);
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var summary = new DashboardSummaryDto();
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);

            IQueryable<DailyManagementSystem.Models.Order> orderQuery = _context.Orders.AsNoTracking();
            IQueryable<DailyManagementSystem.Models.Payment> paymentQuery = _context.Payments.AsNoTracking();
            IQueryable<DailyManagementSystem.Models.DailySpent> expenseQuery = _context.DailySpents.AsNoTracking();

            if (range.start.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.OrderDate >= range.start.Value && o.OrderDate <= range.end!.Value);
                paymentQuery = paymentQuery.Where(p => p.PaymentDate >= range.start.Value && p.PaymentDate <= range.end!.Value);
                expenseQuery = expenseQuery.Where(e => e.SpentDate >= range.start.Value && e.SpentDate <= range.end!.Value);
            }

            var orders = await orderQuery.ToListAsync();
            summary.TotalOrders = orders.Count;
            summary.TotalOrderAmount = orders.Sum(o => o.OrderAmount);

            summary.TotalReceivedAmount = await paymentQuery.SumAsync(p => p.AmountReceived);
            summary.TotalExpenses = await expenseQuery.SumAsync(e => e.Amount);
            
            var orderIds = orders.Select(o => o.OrderId).ToList();
            var paymentsForOrders = await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId.HasValue && orderIds.Contains(p.OrderId.Value))
                .SumAsync(p => p.AmountReceived);
            
            summary.TotalPendingAmount = summary.TotalOrderAmount - paymentsForOrders;
            summary.Profit = summary.TotalReceivedAmount - summary.TotalExpenses;

            return summary;
        }

        public async Task<IEnumerable<ClientMonthlyReportDto>> GetMonthlyClientReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);
            IQueryable<DailyManagementSystem.Models.Order> orderQuery = _context.Orders.AsNoTracking();
            IQueryable<DailyManagementSystem.Models.Payment> paymentQuery = _context.Payments.AsNoTracking();

            if (range.start.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.OrderDate >= range.start.Value && o.OrderDate <= range.end!.Value);
                paymentQuery = paymentQuery.Where(p => p.PaymentDate >= range.start.Value && p.PaymentDate <= range.end!.Value);
            }

            var clientIdsWithOrders = await orderQuery.Select(o => o.ClientId).Distinct().ToListAsync();
            var clientIdsWithPayments = await paymentQuery.Select(p => p.ClientId).Distinct().ToListAsync();
            var activeClientIds = clientIdsWithOrders.Union(clientIdsWithPayments).Distinct().ToList();

            var report = new List<ClientMonthlyReportDto>();
            foreach (var clientId in activeClientIds)
            {
                var clientName = await _context.Clients.Where(c => c.ClientId == clientId).Select(c => c.ClientName).FirstOrDefaultAsync() ?? "Unknown";
                
                var clientOrders = await orderQuery.Where(o => o.ClientId == clientId).ToListAsync();
                var totalOrder = clientOrders.Sum(o => o.OrderAmount);
                var totalPaid = await paymentQuery.Where(p => p.ClientId == clientId).SumAsync(p => p.AmountReceived);
                
                report.Add(new ClientMonthlyReportDto
                {
                    ClientName = clientName,
                    OrderCount = clientOrders.Count,
                    TotalOrderAmount = totalOrder,
                    TotalPaidAmount = totalPaid,
                    PendingAmount = totalOrder - totalPaid
                });
            }

            return report.OrderBy(r => r.ClientName);
        }

        public async Task<IEnumerable<OrderReportDto>> GetMonthlyOrderReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);
            IQueryable<DailyManagementSystem.Models.Order> query = _context.Orders.Include(o => o.Client).AsNoTracking();
            
            if (range.start.HasValue)
            {
                query = query.Where(o => o.OrderDate >= range.start.Value && o.OrderDate <= range.end!.Value);
            }

            return await query
                .OrderBy(o => o.OrderDate)
                .Select(o => new OrderReportDto {
                    Date = o.OrderDate,
                    ClientName = o.Client != null ? o.Client.ClientName : "Unknown",
                    Amount = o.OrderAmount,
                    Status = o.Status
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentReportDto>> GetMonthlyPaymentReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);
            IQueryable<DailyManagementSystem.Models.Payment> query = _context.Payments.Include(p => p.Client).Include(p => p.Order).AsNoTracking();
            
            if (range.start.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= range.start.Value && p.PaymentDate <= range.end!.Value);
            }

            var payments = await query.OrderBy(p => p.PaymentDate).ToListAsync();
            
            return payments.Select(p => new PaymentReportDto {
                Date = p.PaymentDate,
                ClientName = p.Client != null ? p.Client.ClientName : "Unknown",
                OrderName = p.Order != null ? p.Order.OrderName ?? "Order #" + p.OrderId : "Advance",
                BankName = p.BankName,
                Amount = p.AmountReceived
            });
        }

        public async Task<IEnumerable<ExpenseReportDto>> GetMonthlyExpenseReportsAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);
            IQueryable<DailyManagementSystem.Models.DailySpent> query = _context.DailySpents.AsNoTracking();
            
            if (range.start.HasValue)
            {
                query = query.Where(e => e.SpentDate >= range.start.Value && e.SpentDate <= range.end!.Value);
            }

            return await query
                .OrderBy(e => e.SpentDate)
                .Select(e => new ExpenseReportDto {
                    Date = e.SpentDate,
                    Description = e.Description,
                    Category = e.Category,
                    SpentBy = e.SpentBy,
                    Amount = e.Amount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryExpenseDto>> GetMonthlyCategoryExpenseSummaryAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);
            IQueryable<DailyManagementSystem.Models.DailySpent> query = _context.DailySpents.AsNoTracking();
            
            if (range.start.HasValue)
            {
                query = query.Where(e => e.SpentDate >= range.start.Value && e.SpentDate <= range.end!.Value);
            }

            var expenses = await query.ToListAsync();
            return expenses
                .GroupBy(e => e.Category)
                .Select(g => new CategoryExpenseDto {
                    Category = g.Key ?? "General",
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();
        }

        public async Task<IEnumerable<PersonExpenseDto>> GetMonthlyPersonExpenseSummaryAsync(int? startYear, int? startMonth, int? endYear = null, int? endMonth = null)
        {
            var range = GetDateRange(startYear, startMonth, endYear, endMonth);
            IQueryable<DailyManagementSystem.Models.DailySpent> query = _context.DailySpents.AsNoTracking();
            
            if (range.start.HasValue)
            {
                query = query.Where(e => e.SpentDate >= range.start.Value && e.SpentDate <= range.end!.Value);
            }

            var expenses = await query.ToListAsync();
            return expenses
                .GroupBy(e => e.SpentBy)
                .Select(g => new PersonExpenseDto {
                    SpentBy = g.Key ?? "Unknown",
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .OrderByDescending(p => p.TotalAmount)
                .ToList();
        }
    }
}
