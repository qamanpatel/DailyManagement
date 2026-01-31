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
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order.OrderAmount <= 0)
                throw new ArgumentException("Order amount must be greater than zero.");

            order.Status = "Pending";
            order.CreatedAt = DateTime.Now;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(order.OrderId);
            if (existingOrder == null)
                throw new KeyNotFoundException($"Order with ID {order.OrderId} not found.");

            existingOrder.OrderDate = order.OrderDate;
            existingOrder.OrderAmount = order.OrderAmount;
            existingOrder.DeliveredDate = order.DeliveredDate;
            existingOrder.Status = order.Status;
            existingOrder.UpdatedAt = DateTime.Now;

            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();
            return existingOrder;
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            // Check if there are related payments? 
            // The DB Schema says ON DELETE RESTRICT for Client->Orders, but what about Payment->Orders? 
            // Payment->Order is ON DELETE SET NULL. So this is safe. 
            
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Client)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByClientAsync(int clientId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByMonthAsync(int year, int month)
        {
            return await _context.Orders
                .Include(o => o.Client)
                .AsNoTracking()
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task MarkOrderAsDeliveredAsync(int orderId, DateTime deliveredDate)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            order.DeliveredDate = deliveredDate;
            order.Status = "Delivered";
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalOrderAmountByMonthAsync(int year, int month)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate.Year == year && o.OrderDate.Month == month)
                .SumAsync(o => o.OrderAmount);
        }

        public async Task<decimal> GetTotalPendingOrderAmountAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == "Pending")
                .SumAsync(o => o.OrderAmount);
        }
    }
}
