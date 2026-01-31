using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DailyManagementSystem.Models;

namespace DailyManagementSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> AddPaymentAsync(Payment payment);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment> UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByClientAsync(int clientId);
        Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId);
        Task<IEnumerable<Payment>> GetPaymentsByMonthAsync(int year, int month);
        Task<decimal> GetTotalReceivedAmountByMonthAsync(int year, int month);
    }
}
