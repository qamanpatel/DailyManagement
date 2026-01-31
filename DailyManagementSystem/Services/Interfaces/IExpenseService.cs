using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<DailySpent> AddExpenseAsync(DailySpent expense);
        Task<IEnumerable<DailySpent>> GetAllExpensesAsync();
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<DailySpent> UpdateExpenseAsync(DailySpent expense);
        Task DeleteExpenseAsync(int spentId);
        Task<IEnumerable<DailySpent>> GetExpensesByMonthAsync(int year, int month);
        Task<decimal> GetTotalExpenseByMonthAsync(int year, int month);
        Task<IEnumerable<DailySpent>> GetExpensesByCategoryAsync(string category);
    }
}
