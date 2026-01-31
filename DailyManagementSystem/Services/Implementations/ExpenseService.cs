using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DailyManagementSystem.Data;
using DailyManagementSystem.Models;
using DailyManagementSystem.Services.Interfaces;

namespace DailyManagementSystem.Services.Implementations
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;

        public ExpenseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailySpent> AddExpenseAsync(DailySpent expense)
        {
            if (expense.Amount <= 0)
                throw new ArgumentException("Expense amount must be greater than zero.");

            expense.CreatedAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(expense.Category))
            {
                expense.Category = "General";
            }

            _context.DailySpents.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<IEnumerable<DailySpent>> GetAllExpensesAsync()
        {
            return await _context.DailySpents
                .AsNoTracking()
                .OrderByDescending(e => e.SpentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.DailySpents
                .AsNoTracking()
                .Select(e => e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<DailySpent> UpdateExpenseAsync(DailySpent expense)
        {
            var existingExpense = await _context.DailySpents.FindAsync(expense.SpentId);
            if (existingExpense == null)
                throw new KeyNotFoundException($"Expense with ID {expense.SpentId} not found.");

            existingExpense.Description = expense.Description;
            existingExpense.Amount = expense.Amount;
            existingExpense.Category = expense.Category;
            existingExpense.SpentDate = expense.SpentDate;
            existingExpense.UpdatedAt = DateTime.Now;

            _context.DailySpents.Update(existingExpense);
            await _context.SaveChangesAsync();
            return existingExpense;
        }

        public async Task DeleteExpenseAsync(int spentId)
        {
            var expense = await _context.DailySpents.FindAsync(spentId);
            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {spentId} not found.");

            _context.DailySpents.Remove(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DailySpent>> GetExpensesByMonthAsync(int year, int month)
        {
            return await _context.DailySpents
                .AsNoTracking()
                .Where(e => e.SpentDate.Year == year && e.SpentDate.Month == month)
                .OrderByDescending(e => e.SpentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalExpenseByMonthAsync(int year, int month)
        {
            return await _context.DailySpents
                .AsNoTracking()
                .Where(e => e.SpentDate.Year == year && e.SpentDate.Month == month)
                .SumAsync(e => e.Amount);
        }

        public async Task<IEnumerable<DailySpent>> GetExpensesByCategoryAsync(string category)
        {
            return await _context.DailySpents
                .AsNoTracking()
                .Where(e => e.Category.ToLower() == category.ToLower())
                .OrderByDescending(e => e.SpentDate)
                .ToListAsync();
        }
    }
}
