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
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            if (payment.AmountReceived <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");

            // Validation: Prevent overpayment if linked to an Order
            if (payment.OrderId.HasValue)
            {
                var order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId.Value);

                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {payment.OrderId.Value} not found.");

                var totalPaidForOrder = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.OrderId == payment.OrderId)
                    .SumAsync(p => p.AmountReceived);

                if (totalPaidForOrder + payment.AmountReceived > order.OrderAmount)
                {
                    throw new InvalidOperationException($"Payment amount exceeds the remaining order balance. Total Order: {order.OrderAmount}, Paid: {totalPaidForOrder}, Attempted: {payment.AmountReceived}");
                }
            }

            payment.CreatedAt = DateTime.Now;
            
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Client)
                .Include(p => p.Order)
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            var existingPayment = await _context.Payments.FindAsync(payment.PaymentId);
            if (existingPayment == null)
                throw new KeyNotFoundException($"Payment with ID {payment.PaymentId} not found.");

            // Validation: Prevent overpayment if linked to an Order (exclude self from sum)
            if (payment.OrderId.HasValue)
            {
                var order = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == payment.OrderId.Value);

                if (order == null)
                    throw new KeyNotFoundException($"Order with ID {payment.OrderId.Value} not found.");

                var totalPaidForOrder = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.OrderId == payment.OrderId && p.PaymentId != payment.PaymentId)
                    .SumAsync(p => p.AmountReceived);

                if (totalPaidForOrder + payment.AmountReceived > order.OrderAmount)
                {
                    throw new InvalidOperationException($"Update failed: Amount exceeds remaining order balance.");
                }
            }

            existingPayment.AmountReceived = payment.AmountReceived;
            existingPayment.PaymentDate = payment.PaymentDate;
            existingPayment.OrderId = payment.OrderId;
            existingPayment.UpdatedAt = DateTime.Now;

            _context.Payments.Update(existingPayment);
            await _context.SaveChangesAsync();
            return existingPayment;
        }

        public async Task DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByClientAsync(int clientId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .AsNoTracking()
                .Where(p => p.ClientId == clientId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByMonthAsync(int year, int month)
        {
            return await _context.Payments
                .Include(p => p.Client)
                .AsNoTracking()
                .Where(p => p.PaymentDate.Year == year && p.PaymentDate.Month == month)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalReceivedAmountByMonthAsync(int year, int month)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentDate.Year == year && p.PaymentDate.Month == month)
                .SumAsync(p => p.AmountReceived);
        }
    }
}
